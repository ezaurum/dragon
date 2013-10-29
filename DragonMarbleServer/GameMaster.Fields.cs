using System;
using System.Collections.Generic;
using System.Threading;
using Dragon.Message;
using log4net;

namespace DragonMarble
{
    public partial class GameMaster
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

        //turn limit
        public const int TurnLimit = 30;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (GameMaster));

        public static GameBoard OriginalBoard { get; set; }
        private readonly Dictionary<short, Guid> _orderCard = new Dictionary<short, Guid>();
        private readonly EventWaitHandle _receiveMessageWaitHandler = new ManualResetEvent(false);
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
        public int Turn { get; set; }
        public List<StageUnitInfo> OrderedByTurnPlayers { get; set; }
        public IGameMessage CurrentAction { get; set; }

        public bool IsGameStartable
        {
            get { return (Units.Count > 1); }
        }

        private StageUnitInfo CurrentPlayer
        {
            get { return OrderedByTurnPlayers[Turn%Units.Count]; }
        }


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