using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<GamePlayer> Players { get;set;}
        private readonly Dictionary<Int16, Guid> _orderCard = new Dictionary<short, Guid>();
        private GameState _state;
        private List<GamePlayer> _availablePlayers;
        private bool _gameContinue;

        public GameMaster(List<StageTile> tiles) : this()
        {
            Board = new GameBoard(tiles);
        }

        public GameMaster()
        {
            _state = GameState.BeforeInit;
            Id = Guid.NewGuid();
            Players = new List<GamePlayer>();
        }

        public Guid Id { get; set; }

        public bool IsGameStartable
        {
            get { return (Players.Count > 1); }
        }

        public void Join(GamePlayer player)
        {
            Players.Add(player);
            player.StageManager = this;

            //set initailize player message
            InitializePlayerGameMessage idMessage = new InitializePlayerGameMessage
            {
                To = player.Id,
                From = Id
            };

            player.SendingMessage = idMessage;
        }

        public void StartGame()
        {
            Logger.Debug("Start game");

            InitGame();
            SendOrderCardSelectMessage();
        }

        private void SendOrderCardSelectMessage()
        {
            if (GameState.Init != _state)
                throw new InvalidOperationException("State is not initialized.");

            _state = GameState.OrderPlayers;
            GameContinue = true;

            //order
            //At first, select order
            foreach (GamePlayer stageUnit in Players)
            {
                stageUnit.SendingMessage = new OrderCardSelectGameMessage
                {
                    To = stageUnit.Id,
                    From = Id,
                    Actor = Id,
                    NumberOfPlayers = (short)Players.Count,
                    OrderCardSelectState = new List<Boolean> { false, false },
                    SelectedCardNumber = -1
                };
            }

        }

        private void InitGame()
        {
            List<StageUnitInfo> units = Players.Cast<StageUnitInfo>().ToList();

            //send initialize message
            Players.ForEach(p =>
            {
                p.StageManager = this;
                p.SendingMessage = new InitializeGameGameMessage
                {
                    From = Id,
                    To = p.Id,
                    Actor = p.Id,
                    FeeBoostedTiles = Board.FeeBoostedTiles,
                    NumberOfPlayers = (short)units.Count,
                    Units = units
                };
            });

            _availablePlayers = Players;
            
            _state = GameState.Init;
        }

        public void Notify(Guid senderGuid,
            GameMessageType messageType,
            Object messageContent)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Notify from {0}, messageType:{1}"
                    , senderGuid, messageType);
            }

            foreach (GamePlayer p in Players.Where(p => p.Id != senderGuid))
            {
                IDragonMarbleGameMessage message = GameMessageFactory.GetGameMessage(messageType);
                //TODO message content setting neeed.
                p.SendingMessage = message;
                
                    //= new GameMessage(Id, p.Id, senderGuid, messageType, messageContent);
            }
        }

        public void SelectOrder(short foo, GamePlayer gamePlayer)
        {
            _orderCard.Add(foo, gamePlayer.Id);
        }

        public Guid GetId(short foo)
        {
            return _orderCard[foo];
        }

        public void OrderEnd()
        {
            //TODO
            var guid = _orderCard[0];
            Players.ForEach(p =>
            {
                if (p.Id.Equals(guid))
                p.Order = 0;
            });

            var guid1 = _orderCard[1];
            Players.ForEach(p =>
            {
                if (p.Id.Equals(guid1))
                    p.Order = 1;
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
                Console.WriteLine("Here is playerAcitions");
                yield return action;

                //if need, other's reactions
                if (action.NeedOther)
                {
                    foreach (StageUnitInfo targetUnit in action.TargetUnits)
                    {
                        Console.WriteLine("This action need target units action.");
                        GameAction othersAction = new GameAction();
                        yield return othersAction;
                    }
                }
            }
        }

        private IEnumerable<GamePlayer> PlayersOrderByTurn()
        {
            for (Turn = 0; Turn < TurnLimit; Turn++)
            {
                Console.WriteLine("Turn:{0}", Turn + 1);
                yield return CurrentPlayer;
            }
        }

        public int Turn { get; set; }


        private GamePlayer CurrentPlayer
        {
            get { return OrderedByTurnPlayers[Turn % Players.Count]; }
        }

        public List<GamePlayer> OrderedByTurnPlayers{ get; set; }

        public void EndGame()
        {
            Logger.Debug("end game.");
        }



        private void ProcessAction()
        {
            foreach (GameAction action in PlayerActions())
            {
                Logger.Debug("Here is ProcessAction");


                GameAction action1 = action;
                foreach (GamePlayer gamePlayer in Players.Where(p => !p.Id.Equals(action1.Actor.Id)))
                {
                    gamePlayer.SendingMessage = new ActivateTurnGameMessage
                    {
                        To = gamePlayer.Id,
                        From = Id,
                        TurnOwner = action.Actor.Id,
                        ResponseLimit = 50000
                    };
                }

                CurrentAction = action;

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
    }
}