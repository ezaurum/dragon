using System;
using Dragon;

namespace Client.Test
{
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

    public class SimpleMessageFactory : IMessageConverter<SimpleMessage, SimpleMessage>
    {
        public event Action<SimpleMessage, int> ReadCompleted;

        public void Convert(byte[] buffer, int offset, int bytesTransferred)
        {
            var d = new byte[bytesTransferred];
            Buffer.BlockCopy(buffer, offset, d, 0, bytesTransferred);
            var simpleMessage = new SimpleMessage();
            simpleMessage.FromByteArray(d);
            ReadCompleted(simpleMessage, 0);
        }

        public void GetByte(SimpleMessage message, out byte[] messageBytes, out int errorCode)
        {
            messageBytes = message.ToByteArray();
            errorCode = 0;
        }
    }
}