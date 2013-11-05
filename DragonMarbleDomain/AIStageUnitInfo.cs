using DragonMarble.Message;

namespace DragonMarble
{
    public class AIStageUnitInfo : StageUnitInfo {

        public override IDragonMarbleGameMessage SendingMessage
        {
            set { 
                AIMessageProcess(value);
            }
        }

        public override IDragonMarbleGameMessage ReceivedMessage
        {
            set
            {
                if (value.MessageType == GameMessageType.StartGame && IsRoomOwner)
                {
                    StageManager.StartGame(Id);
                }
                else
                {
                    base.ReceivedMessage = value;
                }
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
                case GameMessageType.EveryoneIsReady:
                    ReceivedMessage = new StartGameGameMessage();
                    break;
                case GameMessageType.WaitingRoomInfo:
                    ReadyStateGameMessage dragonMarbleGameMessage = new ReadyStateGameMessage
                    {
                        Actor = Id,
                        Ready = true,
                    };

                    ReceivedMessage = dragonMarbleGameMessage;

                    IsReady = true;

                    StageManager.ReadyNotify(dragonMarbleGameMessage);
                    break;
            }            
        }
    }
}