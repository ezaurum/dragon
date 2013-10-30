using System;
using DragonMarble;
using DragonMarble.Message;

namespace ConsoleTest
{
    public partial class ClientProgram
    {

        private static void InitWaitingRoom(InitializeWaitingRoomGameMessage dragonMarbleGameMessage)
        {
        }

        private static void Join(NewPlayerJoinGameMessage dragonMarbleGameMessage)
        {
            StageUnitInfo stageUnitInfo = new StageUnitInfo
            {
                Id = dragonMarbleGameMessage.PlayerId
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
