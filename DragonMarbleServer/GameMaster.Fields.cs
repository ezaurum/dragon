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
            JustMade = 0,
            WaitingRoom,
            BeforeInit,
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

        private readonly Dictionary<short, Guid> _orderCard = new Dictionary<short, Guid>();
        private readonly EventWaitHandle _receiveMessageWaitHandler = new ManualResetEvent(false);
        private List<StageUnitInfo> _availablePlayers;
        private bool _gameContinue;
        private GameState _state;

        public GameMaster(short playerNumberForPlayer)
        {
            PlayerNumberForPlay = playerNumberForPlayer;
            _state = GameState.JustMade;
            Id = Guid.NewGuid();
            Units = new List<StageUnitInfo>();
        }
        
        public static GameBoard OriginalBoard { get; set; }
        public short PlayerNumberForPlay { get; private set; }
        public static List<StageChanceCardInfo> ChanceCardList = new List<StageChanceCardInfo>();

        public GameBoard Board { get; set; }
        public int Turn { get; set; }

        public IGameMessage CurrentAction { get; set; }

        public List<StageUnitInfo> OrderedByTurnPlayers { get; set; }

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