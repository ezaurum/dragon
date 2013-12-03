using System;
using System.Net.Sockets;
using Dragon;

namespace Client.Test
{
    public static class ClientTestProgram
    {
        static void Main(string[] args)
        {
            var c = new ClientDragonSocket<SimpleMessage>(new SimpleMessageFactory());
            c.ConnectSuccess += (sender, eventArgs) => 
            {
                Console.WriteLine("Connected");

                c.Send(new SimpleMessage());
            };

            c.Disconnected += OnDisconnected;

            c.WriteCompleted += message =>
            {
                Console.WriteLine("Write c");
            };

            c.ReadCompleted += message =>
            {
                Console.WriteLine("Read");
            };

            c.WriteCompleted += message =>
            {
                Console.WriteLine("Sended "+message.ToString());

            };

            c.Connect("127.0.0.1",10008);
            
            Console.ReadKey();
        }

        private static void OnDisconnected(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Console.WriteLine("Disconnected");
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
