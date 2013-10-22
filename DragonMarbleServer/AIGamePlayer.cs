using System;
using Dragon.Interfaces;
using Dragon.Server;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public sealed class AIGamePlayer : StageUnitInfo
    {
        public AIGamePlayer()
            : this(UNIT_COLOR.BLUE, 2000000)
        {   
        }

        public AIGamePlayer(UNIT_COLOR teamColor, int initCapital) : base(teamColor, initCapital)
        {
            ControlMode = ControlModeType.AI_0;
            MessageProcessor = new AIQueuedMessageProcessor();
            Dice = new StageDiceInfo();
        }

        public override IStageManager StageManager
        {
            set
            {
                base.StageManager = value;
                ((AIQueuedMessageProcessor) MessageProcessor).GameMasterId = value.Id;
            }
        }

        internal class AIQueuedMessageProcessor : QueuedMessageProcessor<IDragonMarbleGameMessage>, IDragonMarbleMessageProcessor
        {
            private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public Guid GameMasterId { get; set; }
            public override IGameMessage SendingMessage
            {
                set
                {
                    base.SendingMessage = value;
                    ReceivedMessage = GetMessage(value);
                }
            }

            public override IDragonMarbleGameMessage ReceivedMessage
            {
                set
                {
                    if (null != value)
                        base.ReceivedMessage = value;
                }
            }

            private IDragonMarbleGameMessage GetMessage(IGameMessage message)
            {
                GameMessageType messageType = ((IDragonMarbleGameMessage)message).MessageType;
                IDragonMarbleGameMessage result = null;
                switch (messageType)
                {
                    case GameMessageType.ActivateTurn:
                        result = new RollMoveDiceGameMessage()
                        {
                            Pressed = 10,
                        };
                        break;
                }
                return result;
            }
        }
    }
}