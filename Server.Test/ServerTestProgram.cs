using System;
using System.Net;
using System.Threading;
using Dragon;
using log4net.Config;

namespace Server.Test
{
    /// <summary>
    /// .Test for socket distributor
    /// </summary>
    public static class ServerTestProgram
    {
        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            var s = new SocketDistributor<SimpleMessage>
            {
                Backlog = 20,
                MaximumConnection = 5,
                IpEndpoint = new IPEndPoint(IPAddress.Any, 10008),
                MessageFactory = new SimpleMessageFactory()

            };
            s.Accepted += (sender, eventArgs) =>
            {
                //Something to test
                var userToken = (ServerDragonSocket<SimpleMessage>) eventArgs.UserToken;



                userToken.ReadCompleted += message =>
                {
                    Console.WriteLine("READ " + message);
                    userToken.Send(message);
                    Console.WriteLine("echo " + message);
                }; 

                userToken.Activate();
            };

            Thread.Sleep(1000);
            s.Start();
            Console.ReadKey();
        }
    }

    // 게임 대기방 생성 요청 (client->server)	
    public class SimpleMessage : IMessage
    {
        public int MessageType { get { return 1; } }
        public DateTime PacketTime { get; set; }
        public Byte BoardType;
        public Byte PlayMode;
        public char PlayType;

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[Length];
            int index = 0;
            BitConverter.GetBytes(Length)
            .CopyTo(bytes, index);
            index += sizeof(Int16);
            BitConverter.GetBytes((Int32)MessageType)
            .CopyTo(bytes, index);
            index += sizeof(Int32);
            BitConverter.GetBytes((Int64)PacketTime.ToBinary())
            .CopyTo(bytes, index);
            index += sizeof(Int64);
            bytes[index] = BoardType;
            index++;
            bytes[index] = PlayMode;
            index++;
            bytes[index] = (Byte)PlayType;
            index++;
            return bytes;
        }

        public void FromByteArray(byte[] bytes)
        {
            int index = 6;
            PacketTime = DateTime.FromBinary(BitConverter.ToInt64(bytes, index));
            index += sizeof(Int64);
            BoardType = bytes[index];
            index += sizeof(Byte);
            PlayMode = bytes[index];
            index += sizeof(Byte);
            PlayType = (char)bytes[index];
            index += sizeof(Byte);
        }

        public Int16 Length
        {
            get
            {
                return (Int16)(2 + sizeof(Int32) + sizeof(Int64) + sizeof(Byte) + sizeof(Byte) + sizeof(Byte));
            }
        }
        public override string ToString()
        {
            return string.Format("MessageType: {0}, PacketTime: {1}, BoardType: {2}, PlayMode: {3}, PlayType: {4}, ", MessageType, PacketTime, BoardType, PlayMode, PlayType);
        }
    }

    public class SimpleMessageFactory : IMessageFactory<SimpleMessage>
    {
        public SimpleMessage GetMessage(byte[] bytes)
        {
            return GetMessage(bytes, 0, bytes.Length);
        }

        public SimpleMessage GetMessage(byte[] bytes, int offset, int length)
        {
            var d = new byte[length];
            Buffer.BlockCopy(bytes, offset, d, 0, length);
            var simpleMessage = new SimpleMessage();
            simpleMessage.FromByteArray(d);
            return simpleMessage;
        }
    }
}
