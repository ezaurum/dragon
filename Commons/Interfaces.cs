using System;
using System.Net.Sockets;

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
        Socket Socket { set; }
        SocketAsyncEventArgs WriteEventArgs { set; }
        SocketAsyncEventArgs ReadEventArgs { set; }
        void Send(T message);
    }

    public interface IMessageConverter<T> where T : IMessage
    {
        event MessageEventHandler<T> MessageConverted;
    }

    public interface IMessageFactory<T> where T : IMessage
    {
        static T GetMessage(byte[] bytes);
        static T GetMessage(byte[] bytes, int offset, int length);
    }

    public delegate void MessageEventHandler<T>(T message) where T : IMessage;
}
