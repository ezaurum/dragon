using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Dragon.Client;
using DragonMarble;
using DragonMarble.Message;

namespace ConsoleTest
{
    class ClientProgram
    {
        private static StageUnitInfo _unitInfo;
        static void Main(string[] args)
        {
            OrderCardSelectState = new List<bool>();
            RollMoveDiceGameMessage rollMessage = new RollMoveDiceGameMessage()
            {
                Pressed = new Random().Next(0,int.MaxValue)
            };    

            Unity3DNetworkManager nm = new Unity3DNetworkManager("127.0.0.1", 10008);
            nm.OnAfterMessageReceive += ProcessClientReceivedMessage;
            nm.OnAfterMessageSend += (sender, eventArgs) => Console.WriteLine("Message Sent");

            nm.Start();

            while (true)
            {
                string readLine = Console.ReadLine();
                
                if (readLine.Contains("Q") || readLine.Contains("q"))
                {
                    return;
                }

                if (readLine.Contains("R") || readLine.Contains("r"))
                {

                    nm.Reconnect();
                }

                if (readLine.Contains("D") || readLine.Contains("d"))
                {

                    nm.SendMessage(rollMessage);
                }

                if (readLine.Contains("A") || readLine.Contains("a"))
                {
                    nm.SendMessage(new ActionResultCopyGameMessage());
                }

                if (readLine.Contains("1"))
                {
                    OrderCardSelectState[1] = true;
                    OrderCardSelectGameMessage orderCardSelectGameMessage = new OrderCardSelectGameMessage
                    {
                        SelectedCardNumber = 1,
                        OrderCardSelectState = OrderCardSelectState,
                        NumberOfPlayers = 2
                    };
                    nm.SendMessage(orderCardSelectGameMessage);
                }
                
                if (readLine.Contains("I")) {
                    while (true)
                    {
                        nm.SendMessage(rollMessage);
                    }
                }

                if (readLine.Contains("0"))
                {
                    OrderCardSelectState[0] = true;
                    OrderCardSelectGameMessage orderCardSelectGameMessage = new OrderCardSelectGameMessage
                    {
                        SelectedCardNumber = 0,
                        OrderCardSelectState = OrderCardSelectState,
                        NumberOfPlayers = 2
                    };
                    nm.SendMessage(orderCardSelectGameMessage);
                }
            }
        }

        private static List<bool> OrderCardSelectState {get ; set; }

        private static void ProcessClientReceivedMessage(object o, SocketAsyncEventArgs eventArgs)
        {   
            QueueAsyncClientUserToken token = (QueueAsyncClientUserToken) eventArgs.UserToken;

            Console.WriteLine("Offeset , {0}", eventArgs.Offset);
            Console.WriteLine("Buffer Length , {0}", eventArgs.Buffer.Length);

            short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
            if (messageLength < 6) return;

            byte[] m = new byte[messageLength];
            Buffer.BlockCopy(eventArgs.Buffer, eventArgs.Offset,m, 0, messageLength);
            var dragonMarbleGameMessage = GameMessageFactory.GetGameMessage(m);
            Console.WriteLine("receive , type: {0}, length :{1}", dragonMarbleGameMessage.MessageType, m.Length);
            token.Message = dragonMarbleGameMessage;

            Console.WriteLine("=======================================================================");
            switch (dragonMarbleGameMessage.MessageType)
            { 
                case GameMessageType.OrderCardSelect:
                    
                    OrderCardSelect((OrderCardSelectGameMessage) dragonMarbleGameMessage);
                    break;
                case GameMessageType.InitializeGame:
                    InitGame((InitializeGameGameMessage)dragonMarbleGameMessage);
                    break;
                case GameMessageType.ActivateTurn:
                    ActivateTurn((ActivateTurnGameMessage) dragonMarbleGameMessage);
                    break;
                case GameMessageType.InitializePlayer:
                    InitializePlayerGameMessage ipgm = (InitializePlayerGameMessage)dragonMarbleGameMessage;
                    _unitInfo = new StageUnitInfo
                    {
                        Id = ipgm.PlayerId,
                        ControlMode = StageUnitInfo.ControlModeType.Player
                    };
                    Console.WriteLine("SYSTEM: player initialized : {0}", _unitInfo.Id);
                    break;

            }
            Console.WriteLine("=======================================================================");
            Console.WriteLine("current thread : {0}",Thread.CurrentThread.ManagedThreadId);
            
            
        }

        private static void OrderCardSelect(OrderCardSelectGameMessage m)
        {
            OrderCardSelectState = m.OrderCardSelectState;

            Console.Write("SYSTEM : cards [");
            foreach (bool b in m.OrderCardSelectState)
            {
                Console.Write(",{0}",b);
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
