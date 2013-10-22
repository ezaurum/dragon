using System;

namespace Dragon.Interfaces
{
    public interface IGameMessage
    {
        Int16 Length { get; }
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes);
    }

    
    public interface IGameAction
    {
    }

    public interface IMessageProcessor<T> where T : IGameMessage
    {
        T ReceivedMessage { get; set; }
        IGameMessage SendingMessage { get; set; }
        void ResetMessages();
    }
}