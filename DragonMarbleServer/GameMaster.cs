using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dragon.Message;
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
        private readonly Dictionary<Int16, Guid> _orderCard = new Dictionary<short, Guid>();
        public readonly EventWaitHandle _receiveMessageWaitHandler = new ManualResetEvent(false);
        private List<StageUnitInfo> _availablePlayers;
        private bool _gameContinue;
        private GameState _state;

        public GameMaster(List<StageTileInfo> tiles)
            : this()
        {
            Board = new GameBoard(tiles);
        }

        public GameMaster()
        {
            _state = GameState.BeforeInit;
            Id = Guid.NewGuid();
            Units = new List<StageUnitInfo>();
        }

        public GameBoard Board { get; set; }

        public bool IsGameStartable
        {
            get { return (Units.Count > 1); }
        }

        public int Turn { get; set; }

        private StageUnitInfo CurrentPlayer
        {
            get { return OrderedByTurnPlayers[Turn%Units.Count]; }
        }

        public List<StageUnitInfo> OrderedByTurnPlayers { get; set; }
        public IGameMessage CurrentAction { get; set; }

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

        public List<StageUnitInfo> Units { get; set; }
        public Guid Id { get; set; }

        /// <summary>
        ///     Notify message for every players
        /// </summary>
        /// <param name="message">message to notify</param>
        public void Notify(IDragonMarbleGameMessage message)
        {
            Units.ForEach(p => p.SendingMessage = message);
        }

        public void ActionResultCopySended()
        {
            CheckAllPlayerSendActionResultCopy();
        }

        public void Join(StageUnitInfo player)
        {
            Units.Add(player);
            player.StageManager = this;
            player.Stage = Board;

            //set initailize player message
            player.SendingMessage = new InitializePlayerGameMessage
            {
                PlayerId = player.Id,
                Server = Id
            };
        }

        public void StartGame()
        {
            InitGame();
            SendOrderCardSelectMessage();
            EndOrder();
            Logger.Debug("Starg gmae and");
        }

        private void InitGame()
        {
            _availablePlayers = Units;

            Board.Init();

            //send initialize message
            Notify(new InitializeGameGameMessage
            {
                FeeBoostedTiles = Board.FeeBoostedTiles,
                NumberOfPlayers = (short) Units.Count,
                Units = Units
            });

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

            Notify(new OrderCardSelectGameMessage
            {
                Actor = Id,
                NumberOfPlayers = (short) Units.Count,
                OrderCardSelectState = new List<bool> {false, false},
                SelectedCardNumber = -1
            });


            //TODO
            Notify(new OrderCardSelectGameMessage
            {
                Actor = Id,
                NumberOfPlayers = (short) Units.Count,
                OrderCardSelectState = new List<bool> {true, true},
                SelectedCardNumber = 1
            });
        }

        private void EndOrder()
        {
            Notify(new OrderCardResultGameMessage
            {
                FirstPlayerId = Units[0].Id,
                FirstCardNumber = 1
            });

            Units[0].Order = 0;
            Units[1].Order = 1;
            OrderedByTurnPlayers = Units.OrderBy(player => player.Order).ToList();

            Logger.Debug("End order");

            //playe game is need another thread.
            Task.Factory.StartNew(PlayGame);
        }

        private void PlayGame()
        {
            Logger.Debug("Play Game");
            ProcessAction();
            EndGame();
        }

        private IEnumerable<IGameMessage> PlayerActions()
        {
            Logger.Debug("Player actions");
            foreach (IGameMessage action
                in PlayersOrderByTurn().SelectMany(player => player.Actions()))
            {
                _state = GameState.WaitPlayerAction;

                Logger.DebugFormat("_state:{0}", _state);

                //turn owner's action
                yield return action;
            }
        }

        private IEnumerable<StageUnitInfo> PlayersOrderByTurn()
        {
            for (Turn = 0; Turn < TurnLimit; Turn++)
            {
                Logger.DebugFormat("wait for action result copy Turn:{0}", Turn + 1);

                _receiveMessageWaitHandler.WaitOne();

                Logger.DebugFormat("start Turn:{0}", Turn + 1);

                Notify(CurrentPlayer.ActivateTurn());

                yield return CurrentPlayer;

                _receiveMessageWaitHandler.Reset();
            }
        }

        public void CheckAllPlayerSendActionResultCopy()
        {
            if (_availablePlayers.All(
                availablePlayer => availablePlayer.IsActionResultCopySended
                ))
            {
                _receiveMessageWaitHandler.Set();
            }
        }

        public void EndGame()
        {
            _state = GameState.EndGame;
            Logger.Debug("end game.");
        }

        private void ProcessAction()
        {
            Logger.Debug("Process action");

            foreach (IGameMessage action in PlayerActions())
            {
                Board.GrossAssets = 0;
                Units.ForEach(p => Board.GrossAssets += p.Assets);
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Gross Assets is : {0}", Board.GrossAssets);
                }

                CurrentAction = action;

                _state = GameState.ProcessPlayerAction;
                _availablePlayers.ForEach(p=>p.IsActionResultCopySended = false);

                Notify((IDragonMarbleGameMessage) action);

                //need check game end
            }
        }

        public static List<StageTileInfo> ParseTiles(XDocument doc)
        {
            // Query the data and write out a subset of contacts
            IEnumerable<StageTileInfo> query = doc.Elements("Category").Elements("Stage").Select(c =>
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

                return new StageTileInfo(
                    int.Parse(c.Attribute("Index").Value.ToString()),
                    c.Attribute("Name").Value.ToString(),
                    c.Attribute("Type").Value.ToString(),
                    c.Attribute("TypeValue").Value.ToString(), buyPrices, sellPrices, fees);
            });
            return query.ToList();
        }
    }
}