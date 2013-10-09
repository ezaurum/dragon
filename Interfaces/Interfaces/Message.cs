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

    public interface IMessageParser
    {
        IGameMessage MakeNewMessage<T, TD>(T messageType, TD messageData);
        IGameMessage MakeNewMessage(byte[] buffer, int offset, int messageLength);
    }

    public interface IMessageProcessor
    {
        void ProcessMessage(IGameMessage gameMessage);
    }
}