using System;
using System.Net.Sockets;

namespace Dragon
{
    public interface IGameMessage
    {
        Int16 Length { get; }
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes);
        DateTime PacketTime { get; set; }
    }

    public interface IGameSession : IDisposable
    {
        Guid Id { get; set; }
    }

    public interface IMessageProcessor<T> where T : IGameMessage
    {
        T ReceivedMessage { get; set; }
        T SendingMessage { set; }
    }

    public interface IAuthorizationManager
    {
        void Login(object sender, SocketAsyncEventArgs e);
    }

    public interface ISessionManager
    {
        void RequestSession(object sender, SocketAsyncEventArgs e);
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
