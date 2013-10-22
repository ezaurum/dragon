using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public enum GameState
    {
        BeforeInit = 0,
        Init,
        StartGame,
        OrderPlayers,
        WaitPlayerAction,
        ProcessPlayerAction,
        EndGame
    }

    public class GameMaster : IStageManager
    {

        public const int TurnLimit = 30;

        private static readonly ILog Logger = LogManager.GetLogger(typeof (GameMaster));
        private GameBoard Board { get; set; }
        public List<StageUnitInfo> Players { get;set;}
        private readonly Dictionary<Int16, Guid> _orderCard = new Dictionary<short, Guid>();
        private GameState _state;
        private List<StageUnitInfo> _availablePlayers;
        private bool _gameContinue;

        public GameMaster(List<StageTile> tiles) : this()
        {
            Board = new GameBoard(tiles);
        }

        public GameMaster()
        {
            _state = GameState.BeforeInit;
            Id = Guid.NewGuid();
            Players = new List<StageUnitInfo>();
        }

        public Guid Id { get; set; }

        public bool IsGameStartable
        {
            get { return (Players.Count > 1); }
        }

        public void Join(StageUnitInfo player)
        {
            Players.Add(player);
            player.StageManager = this;
            player.Stage = Board;

            //set initailize player message
            InitializePlayerGameMessage idMessage = new InitializePlayerGameMessage
            {
                PlayerId = player.Id,
                Server = Id
            };

            player.SendingMessage = idMessage;
        }

        public void StartGame()
        {
            Logger.Debug("Start game");
            InitGame();
            SendOrderCardSelectMessage();
            Task.Factory.StartNew(OrderEnd);
            

            //IDragonMarbleGameMessage message = Players[0].ReceivedMessage;
        }

        
        private void InitGame()
        {
            _availablePlayers = Players;

            Board.Init();

            //send initialize message
            Notify(GameMessageInstanceFactory.MakeInitializePlayerMessage, _availablePlayers, Board.FeeBoostedTiles);

            _state = GameState.Init;
        }

        private void SendOrderCardSelectMessage()
        {
            if (GameState.Init != _state)
                throw new InvalidOperationException("State is not initialized.");

            _state = GameState.OrderPlayers;
            GameContinue = true;

            //order
            //At first, select order
            Notify(GameMessageInstanceFactory.OrderCardSelect
                , (short)Players.Count
                , new List<bool> { false, false }
                , (short)-1);
        }

        public void Notify(Func<StageUnitInfo, object[], IDragonMarbleGameMessage> instanceMessage, params object[] parameterObjects)
        {
            Players.ForEach(p =>
            {
                IDragonMarbleGameMessage message = instanceMessage(p, parameterObjects);
                p.SendingMessage = message;
            });
        }
       
        public void OrderEnd()
        {
            Players.ForEach(p =>
            {
                if (p.ControlMode != StageUnitInfo.ControlModeType.Player) return;
                
                IDragonMarbleGameMessage message = p.ReceivedMessage;
                if (message.MessageType == GameMessageType.OrderCardSelect)
                {
                    
                }
            });
            
            Players.ForEach(p =>
            {       if (p.ControlMode == StageUnitInfo.ControlModeType.Player)
                    {
                        p.Order = 0;
                        var dragonMarbleGameMessage = p.ReceivedMessage;
                        //p.ResetMessages();

                    }
                    else
                    {
                        p.Order = 1;
                    }
                
            });

            OrderedByTurnPlayers = Players.OrderBy(player => player.Order).ToList();
            
            PlayGame();
        }

        private void PlayGame()
        {
            ProcessAction();
            EndGame();
        }

        private IEnumerable<GameAction> PlayerActions()
        {
            foreach (GameAction action
                in PlayersOrderByTurn().SelectMany(player => player.Actions()))
            {
                
                //turn owner's action
                
                yield return action;

                //if need, other's reactions
                if (action.NeedOther)
                {
                    foreach (StageUnitInfo targetUnit in action.TargetUnits)
                    {
                        Logger.Debug("This action need target units action.");
                        
                        GameAction othersAction = new GameAction();
                        yield return othersAction;
                    }
                }
            }
        }

        private IEnumerable<StageUnitInfo> PlayersOrderByTurn()
        {
            for (Turn = 0; Turn < TurnLimit; Turn++)
            {
                Logger.DebugFormat("Turn:{0}", Turn + 1);
                
                CurrentPlayer.ActivateTurn();

                Notify(GameMessageInstanceFactory.ActivateTurn, CurrentPlayer.Id);

                yield return CurrentPlayer;
            }
        }

        public int Turn { get; set; }

        private StageUnitInfo CurrentPlayer
        {
            get { return OrderedByTurnPlayers[Turn % Players.Count]; }
        }

        public List<StageUnitInfo> OrderedByTurnPlayers{ get; set; }

        public void EndGame()
        {
            Logger.Debug("end game.");
        }

        private void ProcessAction()
        {
            foreach (GameAction action in PlayerActions())
            {
                Logger.DebugFormat("Here is ProcessAction {0}",action.Type);
                Board.GrossAssets = 0;
                Players.ForEach(p => Board.GrossAssets += p.Assets);
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Gross Assets is : {0}", Board.GrossAssets);
                }
              
                CurrentAction = action;


                Func<StageUnitInfo, object[], IDragonMarbleGameMessage> pa = null;
                switch (action.Type)
                {
                    case GameMessageType.BuyLandRequest:
                        pa = GameMessageInstanceFactory.BuyLandRequest;
                        break;
                    case GameMessageType.RollMoveDiceResult:
                        pa = GameMessageInstanceFactory.RollMoveDiceResult;
                        break;
                }
                
                Notify(pa,action.ArgObjects);

                //need check game end
            }
        }

        public GameAction CurrentAction { get; set; }

        public bool GameContinue
        {
            get
            {
                if (_availablePlayers.Count < 2)
                {
                    Logger.Debug("Everyone else is out.");
                    _gameContinue = false;
                    return _gameContinue;
                }
                if (Turn > 29)
                {
                    Logger.Debug("Turn over.");
                    _gameContinue = false;
                }
                return _gameContinue;
            }
            set { _gameContinue = value; }
        }

        public static List<StageTile> ParseTiles(XDocument doc)
        {
            // Query the data and write out a subset of contacts
            IEnumerable<StageTile> query = doc.Elements("Category").Elements("Stage").Select(c =>
            {
                IEnumerable<XElement> xElements = c.Elements("Price");

                var buyPrices = new int[4];
                var sellPrices = new int[4];
                var fees = new int[4];

                int i = 0;
                foreach (XElement xElement in xElements)
                {
                    buyPrices[i] = int.Parse(xElement.Attribute("BuyPrice").Value.ToString());
                    fees[i] = int.Parse(xElement.Attribute("Fee").Value.ToString());
                    sellPrices[i] = int.Parse(xElement.Attribute("SellPrice").Value.ToString());
                }

                return new StageTile(
                    int.Parse(c.Attribute("Index").Value.ToString()),
                    c.Attribute("Name").Value.ToString(),
                    c.Attribute("Type").Value.ToString(),
                    c.Attribute("TypeValue").Value.ToString(), buyPrices, sellPrices, fees);
            });
            return query.ToList();
        }
    }
}