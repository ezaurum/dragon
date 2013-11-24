using System;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    /// Game message to be converted to bytestream
    /// </summary>
    public interface IGameMessage
    {
        Int16 Length { get; }
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes);
        DateTime PacketTime { get; set; }
    }

    /// <summary>
    /// Session object
    /// </summary>
    public interface IGameSession : IDisposable
    {
        Guid Id { get; set; }
    }

    /// <summary>
    /// message processor
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public interface IMessageProcessor<T> where T : IGameMessage
    {
        T ReceivedMessage { get; set; }
        T SendingMessage { set; }
    }

    public interface IEventProcessor<T> where T : EventArgs
    {
        event EventHandler<T> Success;
        event EventHandler<T> Fail;
        void Trigger(object sender, T e);
    }

    /// <summary>
    /// Authorization manager
    /// </summary>
    public interface IAuthorizationManager
    {
        void Login(object sender, SocketAsyncEventArgs e);
        event EventHandler<SocketAsyncEventArgs> Authorized;
    }

    public interface ISessionManager
    {
        void RequestSession(object sender, SocketAsyncEventArgs e);
        event EventHandler<SocketAsyncEventArgs> SessionAcquired;
    }

    public interface IActionController
    {
        void Init(object sender, SocketAsyncEventArgs e);
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

}
