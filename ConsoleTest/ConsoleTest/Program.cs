using System;
using Dragon.Client;
using DragonMarble.Message;

namespace ConsoleTest
{
    class Program
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

            Unity3DNetworkManager nm = new Unity3DNetworkManager("localhost", 10008);
            
            while (true)
            {
                string readLine = Console.ReadLine();
                
                if (readLine.Contains("Q"))
                {
                    return;
                }

                if (readLine.Contains("D") )
                {

                    nm.SendMessage(rollMessage);
                }
            }
        }
    }
}
