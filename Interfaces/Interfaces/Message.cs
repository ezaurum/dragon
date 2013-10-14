using System;

namespace Dragon.Interfaces
{
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
        Int16 MessageLength { get; }
        byte[] ToByteArray();
        byte[] ToByteArray(byte[] bytes);
    }
}