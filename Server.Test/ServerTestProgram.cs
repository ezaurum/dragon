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
                IpEndpoint = new IPEndPoint(IPAddress.Any, 10008)
            };
            s.Accepted += (sender, eventArgs) =>
            {
                //Something to test
                var userToken = (ServerDragonSocket<SimpleMessage>) eventArgs.UserToken;
            };

            Thread.Sleep(1000);
            s.Start();
            Console.ReadKey();
        }
    }

    public class SimpleMessage : IMessage
    {
        public DateTime PacketTime { get; set; }
        public Guid TurnOwner { get; set; }
        public Int32 ResponseLimit;

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[Length];
            int index = 0;
            BitConverter.GetBytes(Length)
            .CopyTo(bytes, index);
            index += sizeof(Int16);
            BitConverter.GetBytes((Int64)PacketTime.ToBinary())
            .CopyTo(bytes, index);
            index += sizeof(Int64);
            TurnOwner.ToByteArray()
            .CopyTo(bytes, index);
            index += 16;
            BitConverter.GetBytes(ResponseLimit)
            .CopyTo(bytes, index);
            index += sizeof(Int32);
            return bytes;
        }

        public void FromByteArray(byte[] bytes)
        {
            int index = 2;
            PacketTime = DateTime.FromBinary(BitConverter.ToInt64(bytes, index));
            index += sizeof(Int64);
            byte[] tempTurnOwner = new byte[16];
            Buffer.BlockCopy(bytes, index, tempTurnOwner, 0, 16);
            TurnOwner = new Guid(tempTurnOwner);
            index += 16;
            ResponseLimit = BitConverter.ToInt32(bytes, index);
            index += sizeof(Int32);
        }

        public Int16 Length
        {
            get
            {
                return (Int16)(2 + sizeof(Int64) + 16 + sizeof(Int32));
            }
        }
        public override string ToString()
        {
            return string.Format("PacketTime: {0}, TurnOwner: {1}, ResponseLimit: {2}, ", PacketTime, TurnOwner, ResponseLimit);
        }
    }
}
