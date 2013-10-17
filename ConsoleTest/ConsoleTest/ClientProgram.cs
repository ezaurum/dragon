using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Dragon.Client;
using DragonMarble;
using DragonMarble.Message;

namespace ConsoleTest
{
    class ClientProgram
    {
        static int counter;
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
                        OrderCardSelectState = new List<Boolean> {false,true},
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
                        OrderCardSelectState = new List<Boolean> { true, false },
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

            byte[] m = new byte[messageLength];
            Buffer.BlockCopy(eventArgs.Buffer, eventArgs.Offset,m, 0, messageLength);
            Console.WriteLine("reset buffer");
            eventArgs.SetBuffer(new byte[1024],0, 1024);
            Buffer.SetByte(eventArgs.Buffer, eventArgs.Offset, 0);
            
            var dragonMarbleGameMessage = GameMessageFactory.GetGameMessage(m);
            Console.WriteLine("receive , {0}/{1}", dragonMarbleGameMessage.MessageType, m.Length);
            token.Message = dragonMarbleGameMessage;

            switch (dragonMarbleGameMessage.MessageType)
            { 
                case GameMessageType.OrderCardSelect:
                    Console.WriteLine("{0},{1}",
                        ((OrderCardSelectGameMessage)dragonMarbleGameMessage).OrderCardSelectState[0],
                    ((OrderCardSelectGameMessage)dragonMarbleGameMessage).OrderCardSelectState[1])
                    ;
                    break;
                case GameMessageType.InitializeGame:
                    Console.WriteLine("number of players : {0}", ((InitializeGameGameMessage) dragonMarbleGameMessage).NumberOfPlayers);
                    Console.WriteLine("number of players : {0}", ((InitializeGameGameMessage)dragonMarbleGameMessage).Units[0].gold);
                    Console.WriteLine("number of players : {0}", ((InitializeGameGameMessage)dragonMarbleGameMessage).Units[0].Id);
                    Console.WriteLine("number of players : {0}", ((InitializeGameGameMessage)dragonMarbleGameMessage).Units[1].Id);
                    Console.WriteLine("number of players : {0}", ((InitializeGameGameMessage)dragonMarbleGameMessage).Units[1].gold);
                    break;
                    case GameMessageType.ActivateTurn:
                    Console.WriteLine(((ActivateTurnGameMessage)dragonMarbleGameMessage).From);
                    Console.WriteLine(((ActivateTurnGameMessage)dragonMarbleGameMessage).To);
                    Console.WriteLine(((ActivateTurnGameMessage)dragonMarbleGameMessage).TurnOwner);
                    
                    Console.WriteLine(counter++);
                    break;
            }

            Console.WriteLine("current thread : {0}",Thread.CurrentThread.ManagedThreadId);
            
            
        }
    }
}
