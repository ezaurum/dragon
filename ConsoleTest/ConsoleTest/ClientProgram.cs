using System;
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

            token.Message = dragonMarbleGameMessage;
        }
    }
}
