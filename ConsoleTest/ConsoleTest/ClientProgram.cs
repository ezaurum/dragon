using System;
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
                    Content = new RollMoveDiceContentC2S()
                }

            };    

            Unity3DNetworkManager nm = new Unity3DNetworkManager("127.0.0.1", 10008);
            nm.OnAfterMessageReceive +=
                delegate(object o, SocketAsyncEventArgs eventArgs)
                { Console.WriteLine("WTF?"); };
            nm.OnAfterMessageSend += delegate(object sender, SocketAsyncEventArgs eventArgs)
            {
                Console.WriteLine("WTTTTTTTTTTTF");
            };

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
    }
}
