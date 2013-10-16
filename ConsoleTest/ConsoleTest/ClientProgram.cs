using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Dragon.Client;
using DragonMarble.Message;

namespace ConsoleTest
{
    class ClientProgram
    {           
        static void Main(string[] args)
        {
            RollMoveDiceGameMessage rollMessage = new RollMoveDiceGameMessage()
            {
                From = Guid.NewGuid(),
                To = Guid.NewGuid(),
                Pressed = new Random().Next(0,int.MaxValue)
            };    

            Unity3DNetworkManager nm = new Unity3DNetworkManager("127.0.0.1", 10008);
            nm.OnAfterMessageReceive += ProcessClientReceivedMessage;
            nm.OnAfterMessageSend += (sender, eventArgs) => Console.WriteLine("Message Sent");

            nm.Start();
            
            while (true)
            {
                string readLine = Console.ReadLine();
                
                if (readLine.Contains("Q"))
                {
                    return;
                }

                if (readLine.Contains("R"))
                {

                    nm.Reconnect();
                }

                if (readLine.Contains("D") )
                {

                    nm.SendMessage(rollMessage);
                }

                if (readLine.Contains("1"))
                {
                    OrderCardSelectGameMessage orderCardSelectGameMessage = new OrderCardSelectGameMessage
                    {
                        From = Guid.NewGuid(),
                        To = Guid.NewGuid(),
                        SelectedCardNumber = 1,
                        OrderCardSelectState = new List<Int16> {-1,2},
                        NumberOfPlayers = 2
                    };
                    nm.SendMessage(orderCardSelectGameMessage);
                }

                if (readLine.Contains("0"))
                {
                    OrderCardSelectGameMessage orderCardSelectGameMessage = new OrderCardSelectGameMessage
                    {
                        From = Guid.NewGuid(),
                        To = Guid.NewGuid(),
                        SelectedCardNumber = 0,
                        OrderCardSelectState = new List<Int16> {2,-1},
                        NumberOfPlayers = 2
                    };
                    nm.SendMessage(orderCardSelectGameMessage);
                }
            }
        }

        private static void ProcessClientReceivedMessage(object o, SocketAsyncEventArgs eventArgs)
        {   
            QueueAsyncClientUserToken token = eventArgs.UserToken as QueueAsyncClientUserToken;

            Console.WriteLine("Offeset , {0}", eventArgs.Offset);
            Console.WriteLine("Buffer Length , {0}", eventArgs.Buffer.Length);

            short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
            if (messageLength < 32) return;
            
                byte[] m = eventArgs.Buffer.Skip(eventArgs.Offset).Take(messageLength).ToArray();
                Console.WriteLine("receive , {0}", m.Length);
            var dragonMarbleGameMessage = GameMessageFactory.GetGameMessage(m);


            Console.WriteLine("receive , {0}", dragonMarbleGameMessage.MessageType);

            switch (dragonMarbleGameMessage.MessageType)
            { case GameMessageType.OrderCardSelect:
                    Console.WriteLine("{0},{1}",
                        ((OrderCardSelectGameMessage)dragonMarbleGameMessage).OrderCardSelectState[0],
                    ((OrderCardSelectGameMessage)dragonMarbleGameMessage).OrderCardSelectState[1])
                    ;
                    break;
            }
            

            token.Message = dragonMarbleGameMessage;
        }
    }
}
