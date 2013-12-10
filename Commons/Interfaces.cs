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
        DateTime PacketTime { get; set; }
    }   
    
    public interface IDragonSocket<T> where T : IMessage
    {        
        event MessageEventHandler<T> ReadCompleted;
        event MessageEventHandler<T> WriteCompleted;
        event VoidMessageEventHandler Disconnected;
        void Send(T message);
        void Activate();
        void Deactivate();
    }

    public interface IMessageConverter<out T> where T : IMessage
    {
        event MessageEventHandler<T> MessageConverted;
    }

    /// <summary>
    /// Need to be singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageFactory<T> where T : IMessage
    {
        T GetMessage(byte[] bytes);
        T GetMessage(byte[] bytes, int offset, int length);
    }

    public delegate void MessageEventHandler<in T>(T message) where T : IMessage;

    public delegate void VoidMessageEventHandler();
}
