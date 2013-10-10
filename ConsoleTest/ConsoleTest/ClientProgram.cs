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
            GameMessage rollMessage = new GameMessage()
            {
                Header = new GameMessageHeader()
                {
                    From = Guid.NewGuid(),
                    To = Guid.NewGuid()

                },
                Body = new GameMessageBody()
                {
                    MessageType = GameMessageType.Roll,
                    Content = new RollMoveDiceContentC2S
                    {
                        Pressed = new Random().Next(0,int.MaxValue)
                    }
                }

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
            
            short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
            byte[] m = eventArgs.Buffer.Skip(eventArgs.Offset).Take(messageLength).ToArray();

            GameMessage initGameMessage = GameMessage.InitGameMessage(m, GameMessageFlowType.S2C);

            Console.WriteLine("receive , {0}", initGameMessage.MessageType);

            token.Message = initGameMessage;
        }
    }
}
