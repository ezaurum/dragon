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

    /// <summary>
    /// message processor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageProcessor<T> where T : IMessage
    {
        T ReceivedMessage { get; set; }
        T SendingMessage { set; }
    }

    /// <summary>
    /// A pool object for some Template inherit EventArgs
    /// </summary>
    /// <typeparam name="T">EventArgs</typeparam>
    public interface IEventArgsPool<T> where T : EventArgs
    {
        /// <summary>
        /// Retrun object to pool
        /// </summary>
        /// <param name="item"></param>
        void Push(T item);
        
        /// <summary>
        /// Get an object from pool
        /// </summary>
        /// <returns></returns>
        T Pop();
        
        /// <summary>
        /// EventHandler called when EventArgs Complete
        /// </summary>
        event EventHandler<T> Completed;

        /// <summary>
        /// Make actual object in queue
        /// </summary>
        /// <param name="capacity"></param>
        void Prepare(int capacity);
    }

    public interface ISession<T> where T : IMessage
    {
        Socket Socket { get; set; }
        SocketAsyncEventArgs ReadEventArgs { get; set; }
        SocketAsyncEventArgs WriteEventArgs { get; set; }
        IMessageProcessor<T> MessageProcessor { get; set; }
    }

}
