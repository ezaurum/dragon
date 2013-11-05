using System;
using DragonMarble;
using DragonMarble.Message;

namespace ConsoleTest
{
    public partial class ClientProgram
    {
        private static void SwitchMessage(IDragonMarbleGameMessage dragonMarbleGameMessage)
        {
            switch (dragonMarbleGameMessage.MessageType)
            {
                case GameMessageType.OrderCardSelect:
                    OrderCardSelect((OrderCardSelectGameMessage)dragonMarbleGameMessage);
                    break;
                case GameMessageType.InitializeGame:
                    InitGame((InitializeGameGameMessage)dragonMarbleGameMessage);
                    break;
                case GameMessageType.ActivateTurn:
                    ActivateTurn((ActivateTurnGameMessage)dragonMarbleGameMessage);
                    break;
                case GameMessageType.InitializePlayer:
                    InitPlayer(dragonMarbleGameMessage);
                    break;
                case GameMessageType.GoToWaitingRoom:
                    Join((GoToWaitingRoomGameMessage)dragonMarbleGameMessage);
                    break;
                case GameMessageType.WaitingRoomInfo:
                    InitWaitingRoom((WaitingRoomInfoGameMessage)dragonMarbleGameMessage);
                    break;
                case GameMessageType.EveryoneIsReady:

                    break;
                case GameMessageType.AssignRoomOwner:
                    if (_unitInfo.Id == ((AssignRoomOwnerGameMessage) dragonMarbleGameMessage).RoomOwner)
                    {
                        _unitInfo.IsRoomOwner = true;
                    }
                    break;
                case GameMessageType.ReadyState:
                    SetReadyState((ReadyStateGameMessage) dragonMarbleGameMessage);
                    break;
            }
        }

        private static void SetReadyState(ReadyStateGameMessage readyStateGameMessage)
        {
            StageUnitInfo stageUnitInfo = _units[readyStateGameMessage.Actor];
            stageUnitInfo.IsReady
                = readyStateGameMessage.Ready;

            if (stageUnitInfo.IsReady)
            {
                Console.WriteLine("{0} is ready.", stageUnitInfo.Id);
            }
            else
            {
                Console.WriteLine("{0} is not ready.", stageUnitInfo.Id);
            }
        }

        private static void InitWaitingRoom(WaitingRoomInfoGameMessage dragonMarbleGameMessage)
        {
        }

        private static void Join(GoToWaitingRoomGameMessage dragonMarbleGameMessage)
        {
            StageUnitInfo stageUnitInfo = new StageUnitInfo
            {
                //Id = dragonMarbleGameMessage..PlayerId
            };
            _units.Add(stageUnitInfo.Id, stageUnitInfo);
            Console.WriteLine("SYSTEM: new Player {0} joined.", stageUnitInfo.Id);
        }

        private static void InitPlayer(IDragonMarbleGameMessage dragonMarbleGameMessage)
        {
            InitializePlayerGameMessage ipgm = (InitializePlayerGameMessage)dragonMarbleGameMessage;
            _unitInfo = new StageUnitInfo
            {
                Id = ipgm.PlayerId,
                ControlMode = StageUnitInfo.ControlModeType.Player
            };

            _units.Add(ipgm.PlayerId, _unitInfo);

            _server = ipgm.Server;
            Console.WriteLine("SYSTEM: player initialized : {0}", _unitInfo.Id);
        }

        private static void OrderCardSelect(OrderCardSelectGameMessage m)
        {
            OrderCardSelectState = m.OrderCardSelectState;

            Console.Write("SYSTEM : cards [");
            foreach (bool b in m.OrderCardSelectState)
            {
                Console.Write(",{0}", b);
            }
            Console.WriteLine("]");
        }

        private static void ActivateTurn(ActivateTurnGameMessage dragonMarbleGameMessage)
        {
            Console.WriteLine(dragonMarbleGameMessage.TurnOwner);
            if (dragonMarbleGameMessage.TurnOwner == _unitInfo.Id)
            {
                Console.WriteLine("SYSTEM: My turn!");
            }
        }

        private static void InitGame(InitializeGameGameMessage initializeGameGameMessage)
        {
            Console.WriteLine("SYSTEM: {0} players", initializeGameGameMessage.NumberOfPlayers);
            foreach (StageUnitInfo info in initializeGameGameMessage.Units)
            {
                Console.WriteLine("SYSTEM : {0}, {1},{2}", info.Id, info.UnitColor, info.Gold);
            }
        }
    }
}
