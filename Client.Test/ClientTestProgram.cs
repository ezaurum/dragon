using System;
using System.Threading;
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

            c.ReadCompleted += message =>
            {                
                Console.WriteLine("Read " +message);

                Thread.Sleep(500);
                if(c.State < DragonSocket<SimpleMessage>.SocketState.Inactive) c.Send(new SimpleMessage());
            };

            c.Connect("127.0.0.1",10008); 
            Console.ReadKey();
            c.Disconnect(); 
            Console.ReadKey();
            Console.WriteLine("We're connecting.........");
            c.Connect("127.0.0.1", 10008); 
            Console.ReadKey(); 
        }

        private static void OnDisconnected()
        {
            Console.WriteLine("Disconnected");
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
            PlayType = (char) bytes[index];
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

    public class SimpleMessage2 : IMessage
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
