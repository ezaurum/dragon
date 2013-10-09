using System;

namespace DragonMarble
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
    }

    public class GameMessageBody : IGameMessageBody
    {
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[GameMessageHeader.HeaderLength + Length];
            return bytes;
        }

        public int Length { get; set; }
    }

    public class GameMessageHeader : IGameMessageHeader
    {
        public const short HeaderLength = SecondGuidIndex + 16;
        public const short LengthMarkerLength = sizeof(Int16);
        public const short FirstGuidIndex = LengthMarkerLength;
        public const short SecondGuidIndex = LengthMarkerLength + 16;
        public Int16 Length { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }

        public byte[] ToByteArray(byte[] bytes)
        {
            BitConverter.GetBytes(Length).CopyTo(bytes, 0);
            From.ToByteArray().CopyTo(bytes, LengthMarkerLength);
            To.ToByteArray().CopyTo(bytes, SecondGuidIndex);
            return bytes;
        }

        public byte[] ToByteArray()
        {
            return ToByteArray(new byte[HeaderLength]);
        }
    }

    public interface IGameMessage
    {
        IGameMessageHeader Header { get; set; }
        IGameMessageBody Body { get; set; }
        byte[] ToByteArray();
    }

    public interface IGameMessageBody
    {
        byte[] ToByteArray();
    }

    public interface IGameMessageHeader
    {
        Int16 Length { get; set; }
        Guid From { get; set; }
        Guid To { get; set; }
        byte[] ToByteArray();
        byte[] ToByteArray(byte[] bytes);
    }
}