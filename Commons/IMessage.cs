using System;

namespace Dragon
{
    /// <summary>
    /// Game message to be converted to bytestream
    /// </summary>
    public interface IMessage
    {
        Int16 Length { get; }
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes);
    }
}