using System;
using Dragon.Interfaces;

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

    public class GameMessageHeader : IGameMessageHeader
    {
        public const Int16 LengthMarkerLength = sizeof(Int16);
        public const Int16 FirstGuidIndex = LengthMarkerLength;
        public const Int16 SecondGuidIndex = LengthMarkerLength + 16;
        public Int16 Length { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
        public const Int16 HeaderLength = SecondGuidIndex + 16;


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
}