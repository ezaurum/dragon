using System;
using Dragon.Interfaces;
using Dragon.Server;
using log4net;

namespace DragonMarble
{
    public class GamePlayer :StageUnit
    {   
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GamePlayer));
        private QueuedMessageProcessor _token;

        public StageUnit Unit { get; set; }
        public GameMaster GameMaster { get; set; }

        public Guid Id
        {
            get
            {
                return Info.Id;
            }
            set
            {
                Info.Id = value;
            }
        }

        public QueuedMessageProcessor Token
        {
            set
            {
                _token = value;
            }
        }

        public GamePlayer() 
            :base(StageUnitInfo.TEAM_COLOR.RED, 1000000)
        {
            Id = Guid.NewGuid();
        }

        public IGameMessage ReceivedMessage
        {
            get
            {
                return _token.ReceivedMessage;
            }
        }

        public IGameMessage SendingMessage
        {
            set 
            {
                _token.SendingMessage = value;
            }
        }
    }
}