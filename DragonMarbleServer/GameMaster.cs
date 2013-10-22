using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dragon.Interfaces;
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
            player.SendingMessage = new InitializePlayerGameMessage
            {
                PlayerId = player.Id,
                Server = Id
            }; 
        }

        public void StartGame()
        {
            Logger.Debug("Start game");
            InitGame();
            SendOrderCardSelectMessage();

            Task.Factory.StartNew(PlayGame);

            //Task.Factory.StartNew(OrderEnd);

            //Task<bool>[] tasks = new Task<bool>[1];

            //Func<object, bool> 
            /*          static void Main()
    {
        // Define a delegate that prints and returns the system tick count
        Func<object, int> action = (object obj) =>
        {
            int i = (int)obj;

            // Make each thread sleep a different time in order to return a different tick count
            Thread.Sleep(i * 100);

            // The tasks that receive an argument between 2 and 5 throw exceptions 
            if (2 <= i && i <= 5)
            {
                throw new InvalidOperationException("SIMULATED EXCEPTION");
            }

            int tickCount = Environment.TickCount;
            Console.WriteLine("Task={0}, i={1}, TickCount={2}, Thread={3}", Task.CurrentId, i, tickCount, Thread.CurrentThread.ManagedThreadId);

            return tickCount;
        };

        const int n = 10;

        // Construct started tasks
        Task<int>[] tasks = new Task<int>[n];
        for (int i = 0; i < n; i++)
        {
            tasks[i] = Task<int>.Factory.StartNew(action, i);
        }

        // Exceptions thrown by tasks will be propagated to the main thread 
        // while it waits for the tasks. The actual exceptions will be wrapped in AggregateException. 
        try
        {
            // Wait for all the tasks to finish.
            Task.WaitAll(tasks);

            // We should never get to this point
            Console.WriteLine("WaitAll() has not thrown exceptions. THIS WAS NOT EXPECTED.");
        }
        catch (AggregateException e)
        {
            Console.WriteLine("\nThe following exceptions have been thrown by WaitAll(): (THIS WAS EXPECTED)");
            for (int j = 0; j < e.InnerExceptions.Count; j++)
            {
                Console.WriteLine("\n-------------------------------------------------\n{0}", e.InnerExceptions[j].ToString());
            }
        }
            */
            //PlayGame();


            //IDragonMarbleGameMessage message = Players[0].ReceivedMessage;
            //OrderEnd();
        }

        
        private void InitGame()
        {
            _availablePlayers = Players;

            Board.Init();

            //send initialize message
            Notify(new InitializeGameGameMessage
            {
                FeeBoostedTiles = Board.FeeBoostedTiles,
                NumberOfPlayers = (short)Players.Count,
                Units = Players
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
                NumberOfPlayers = (short)Players.Count,
                OrderCardSelectState = new List<bool> { false, false },
                SelectedCardNumber = -1
            });

            //TODO
            Notify(new OrderCardSelectGameMessage
            {
                Actor = Id,
                NumberOfPlayers = (short)Players.Count,
                OrderCardSelectState = new List<bool> { true, true },
                SelectedCardNumber = 1
            });

            

            Notify(new OrderCardResultGameMessage()
            {
                FirstPlayerId = Players[0].Id,
                FirstCardNumber = 1
            });

            Players[0].Order = 0;
            Players[1].Order = 1;
            OrderedByTurnPlayers = Players.OrderBy(player => player.Order).ToList();
        }

        public void Notify(Func<StageUnitInfo, object[], IDragonMarbleGameMessage> instanceMessage, params object[] parameterObjects)
        {
            Players.ForEach(p =>
            {
                IDragonMarbleGameMessage message = instanceMessage(p, parameterObjects);
                p.SendingMessage = message;
            });
        }

        public void Notify(IDragonMarbleGameMessage message)
        {
            Players.ForEach(p => p.SendingMessage = message);
        }
      
        private void PlayGame()
        {
            ProcessAction();
            EndGame();
        }

        private IEnumerable<IGameAction> PlayerActions()
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

                Notify(new ActivateTurnGameMessage
                {
                    TurnOwner = CurrentPlayer.Id,
                    ResponseLimit = 50000
                });

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

                Notify(action.Message);

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