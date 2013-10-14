using System;
using System.Collections.Generic;

using System.Linq;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public enum StageState
    {
        BeforeInit = 0,
        InitStage,
        StartGame,
        OrderPlayers,
        WaitPlayerAction,
        ProcessPlayerAction,
        EndGame
    }
    
    public class StageManager
    {
        private readonly Guid _id;
        public const int TurnLimit = 30;

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(StageManager));
        
        private readonly IDictionary<int, int> _first 
            = new Dictionary<int, int>() 
            { { 1, -1 }, { 2, -1 }, { 3, -1 }, { 4, -1 } };
        private readonly List<GamePlayer> _players;
        private readonly IList<StageTile> _tiles;
        
        private StageUnit[] _availablePlayers;
        private bool _gameContinue;
        private StageState _state;
        private IDictionary<string, GameMessage> messages;
        private bool _playerOreded;

        public int Turn { get; set; }

        public IList<StageTile> Tiles
        {
            get { return _tiles; }
        }

        public bool GameContinue
        {
            get
            {
                if (_availablePlayers.Length < 2)
                {
                    Console.WriteLine("Turn ");
                    _gameContinue = false;
                    return _gameContinue;
                }
                if (Turn > 29)
                {
                    Console.WriteLine("Turn over.");
                    _gameContinue = false;
                }
                return _gameContinue;
            }
            set { _gameContinue = value; }
        }

        public StageUnit[] OrderedByCapitalPlayers { get; set; }
        public StageUnit[] OrderedByTurnPlayers { get; set; }

        public StageManager(IList<StageTile> tiles, List<GamePlayer> players)
        {
            Console.WriteLine("Stage Manager start");
            _state = StageState.BeforeInit;
            _tiles = tiles;
            _players = players;
            _id = Guid.NewGuid();
        }
        
        public void InitGame()
        {
            List<StageUnitInfo> units 
                = _players.Select(stageUnit => stageUnit.Info).ToList();

            foreach (GamePlayer stageUnit in _players)
            {   
                stageUnit.StageManager = this;

                
                stageUnit.SendingMessage = new GameMessage
                {
                    Header = new GameMessageHeader
                    {
                        From = Id,
                        To = stageUnit.Id
                    }, Body = new GameMessageBody
                    {
                        MessageType = GameMessageType.InitializeGame,
                        Content = new InitializeGameContent
                        {
                            NumberOfPlayers = units.Count,
                            FeeBoostedTiles = new[]{3,31,29,4},
                            Units = units
                        }
                    }
                };
            }

            _state = StageState.InitStage;
        }


        public void StartGame()
        {
            if (StageState.InitStage != _state)
                throw new InvalidOperationException("State is not initialized.");

            _state = StageState.StartGame;
            GameContinue = true;

            //order
            //OrderPlayers();
            
        }

        private void SendOrderMessageToPlayers()
        {
            Console.WriteLine("Ordering player");
            _state = StageState.OrderPlayers;
            foreach (GamePlayer stageUnit in _players)
            {
                stageUnit.StartTurn(Turn, GameMessageType.OrderCardSelect);
            }
        }

        public void OrderPlayers()
        {

            //At first, select order

            //TODO dummy
            _players[1].Order = 1;

            //init game


            OrderedByTurnPlayers = (from player in _players orderby player.Order select player).ToArray();
            int j = 0;
            foreach (StageUnit orderedByCapitalPlayer in OrderedByTurnPlayers)
            {
                orderedByCapitalPlayer.Order = j++;
                Console.WriteLine(orderedByCapitalPlayer.TeamColor + ", " + orderedByCapitalPlayer.Order);
            }

            OrderedByCapitalPlayers = (from player in _players orderby player.Capital select player).ToArray();
            int i = 0;
            foreach (StageUnit orderedByCapitalPlayer in OrderedByCapitalPlayers)
            {
                orderedByCapitalPlayer.CapitalOrder = i++;
                Console.WriteLine(orderedByCapitalPlayer.TeamColor + ", " + orderedByCapitalPlayer.Order);
            } 
        }

        public void PlayGame()
        {
            ProcessAction();
            EndGame();
        }

        private void ProcessAction()
        {   
            foreach (GameAction action in PlayerActions())
            {
                Console.WriteLine("Here is ProcessAction");
                CurrentAction = action;

                if (Result.TargetUnits != null)
                {
                    Result.TargetUnits.ForEach(p => p.Result = Result);
                }

                if (Result.TargetTiles != null)
                {
                    Result.TargetTiles.ForEach(t => t.Result = Result);
                }

                //need check game end
            }
        }

        public GameAction CurrentAction { get; set; }


        private void FindWinner()
        {
        }

        public GameActionResult Result
        {
            get
            {
                return GetResult(CurrentAction);
            }
        }

        public GameActionResult GetResult(GameAction action)
        {
            switch (action.Type)
            {
                case GameMessageType.OrderCardSelect:
                    return OrderPlayers(action);
                case GameMessageType.RollDice:
                    break;
            }
            return DoGameAction(action);
        }

        private GameActionResult OrderPlayers(GameAction action)
        {
            int unordered = 0;
            int unSelectedCard = 0;
            var result = new GameActionResult(action);

            //check unorderd
            for (int i = 1; i < 5; i++)
            {
                if (_first[i] < 0)
                {
                    unordered++;
                    unSelectedCard = i;
                }
            }

            if (unordered == 1)
            {
                //player number
                for (int playerNumber = 0;
                    playerNumber < _players.Count;
                    playerNumber++)
                {
                    if (!_first.Values.Contains(playerNumber))
                    {
                        _first[unSelectedCard] = playerNumber;
                    }
                }

                _playerOreded = true;
            }

            if (_first[action.Selected] > -1
                && _first[action.Selected] != action.PlayerNumber)
            {
                result.Success = false;
            }
            else
            {
                _first[action.Selected] = action.PlayerNumber;
                result.Success = true;
            }
            return result;
        }

        private GameActionResult DoGameAction(GameAction action)
        {
            int playerNumber = action.PlayerNumber;
            Console.WriteLine("Player {0} Do action something...",playerNumber);
            var gameActionResult = new GameActionResult(action)
            {
                EffectedPlayers = new List<int>()
                {
                    0,
                    1
                }
            };
            return gameActionResult;
        }

        private IEnumerable<StageUnit> PlayersOrderByTurn()
        {
            for (Turn = 0; Turn < TurnLimit; Turn++)
            {
                Console.WriteLine("Turn:{0}",Turn +1);
                yield return CurrentPlayer;
            }
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
                    foreach (StageUnit targetUnit in action.TargetUnits)
                    {
                        Console.WriteLine("This action need target units action.");
                        GameAction othersAction = new GameAction();
                        yield return othersAction;
                    }
                }
            }
        }

        private StageUnit CurrentPlayer
        {
            get { return OrderedByTurnPlayers[Turn % _players.Count]; }
        }

        public void EndGame()
        {
            Console.WriteLine("end game.");
            FindWinner();
        }
    }
}