using System;
using Dragon;
using Dragon.Client;
using Dragon.Interfaces;
using DragonMarble.Message;

namespace ConsoleTest
{
    class Program
    {
        internal class TestParser : IMessageParser
        {
            public void SetMessageBody(IGameMessageBody body)
            {
                throw new NotImplementedException();
            }

            public IGameMessage MakeNewMessage<T, TD>(T messageType, TD messageData)
            {
                GameMessage message = new GameMessage();
                message.Header = new GameMessageHeader()
                {
                    From = Guid.NewGuid(),
                    To = Guid.NewGuid()

                };
                message.Body = new GameMessageBody()
                {
                    MessageType = GameMessageType.Roll,
                    Content = new RollMoveDiceContentC2S()
                };
                
                return message;
            }

            public IGameMessage MakeNewMessage(byte[] buffer, int offset, int messageLength)
            {
                throw new NotImplementedException();
            }
        }
        static void Main(string[] args)
        {
            IMessageParser parser = new TestParser();
            IMessageProcessor processor = new TestProcessor();
            Unity3DNetworkManager nm = new Unity3DNetworkManager("localhost", 10008);
            IGameMessageBody body = new TestGameMessageBody();

            IGameMessage gameMessage = parser.MakeNewMessage(0,"test");
            
            while (true)
            {
                string readLine = Console.ReadLine();
                if (readLine.Contains("Q"))
                {
                    return;
                }

                if (readLine.Contains("D") )
                {
                    
                    nm.SendMessage(gameMessage);
                }
            }
        }
    }

    internal class TestProcessor : IMessageProcessor
    {
        public void ProcessMessage(IGameMessage gameMessage)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestGameMessageBody : IGameMessageBody
    {
        public byte[] ToByteArray()
        {
            byte[] result = new byte[8 + 34];
            BitConverter.GetBytes(0).CopyTo(result,0);
            BitConverter.GetBytes(1).CopyTo(result, 4);
            return result;
        }
    }
}
