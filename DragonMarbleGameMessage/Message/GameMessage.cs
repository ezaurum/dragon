using System;
using Dragon.Interfaces;

namespace DragonMarble.Message
{
    public class GameMessage : IGameMessage
    {
        public IGameMessageHeader Header { get; set; }
        public IGameMessageBody Body { get; set; }

        public byte[] ToByteArray()
        {
            byte[] result = Body.ToByteArray();
            return Header.ToByteArray(result);
        }

        public GameMessageType MessageType
        {
            get
            {
                return ((GameMessageBody) Body).MessageType;
            }
        }

        public IGameMessageContent Content
        {
            get
            {
                return ((GameMessageBody) Body).Content;
            }
            set
            {
                ((GameMessageBody)Body).Content = value;
            }
        }
    }

    public class GameMessageHeader : IGameMessageHeader
    {
        public const Int16 LengthMarkerLength = sizeof(Int16);
        public const Int16 FirstGuidIndex = LengthMarkerLength;
        public const Int16 SecondGuidIndex = LengthMarkerLength + 16;
        public Guid From { get; set; }
        public Guid To { get; set; }
        public const Int16 HeaderLength = SecondGuidIndex + 16;


        public byte[] ToByteArray(byte[] bytes)
        {
            MessageLength = (short) bytes.Length;
            BitConverter.GetBytes(MessageLength).CopyTo(bytes, 0);
            From.ToByteArray().CopyTo(bytes, LengthMarkerLength);
            To.ToByteArray().CopyTo(bytes, SecondGuidIndex);
            return bytes;
        }

        public short MessageLength { get; set; }

        public byte[] ToByteArray()
        {
            return ToByteArray(new byte[HeaderLength]);
        }
    }

    public class GameMessageBody : IGameMessageBody
    {
        public GameMessageType MessageType { get; set; }
        public IGameMessageContent Content { get; set; }

        private const int MessageTypeSize = sizeof(Int32);
        
        public byte[] ToByteArray()
        {
            byte[] contents = Content.ToByteArray();
            byte[] bytes = new byte[contents.Length + GameMessageHeader.HeaderLength + MessageTypeSize];
            
            BitConverter.GetBytes((int)MessageType).CopyTo(bytes, GameMessageHeader.HeaderLength);
            contents.CopyTo(bytes, GameMessageHeader.HeaderLength + MessageTypeSize);
            return bytes;
        }
    }

    public interface IGameMessageContent
    {
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes, int index = 0);
    }
}