using DragonMarble.Message;

namespace DragonMarble
{
    public class AIStageUnitInfo : StageUnitInfo {

        public override IDragonMarbleGameMessage SendingMessage
        {
            set { 
                MessageProcessor.SendingMessage = value;
                AIMessageProcess(value);
            }
        }
		
        private void AIMessageProcess(IDragonMarbleGameMessage message) {
            switch ( message.MessageType ) {
                case GameMessageType.ActivateTurn:
                    ReceivedMessage = new RollMoveDiceGameMessage
                    {
                        Actor = Id,
                        Pressed = RandomUtil.Next(0f, 1f)
                    };
                    break;
                case GameMessageType.RollMoveDiceResult:
                    break;
            }

            MessageProcessor.ReceivedMessage = message;
        }
    }
}