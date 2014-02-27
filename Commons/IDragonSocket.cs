using System;
using System.Net;
using System.Net.Sockets;

namespace Dragon
{
    public enum SocketState
    { 
        Disposed = 0,
        Disconnected = 2,
        Initialized = 4,
        Active = 8, 
    }

    public interface IDragonSocket<in TReq, TAck> : IDragonSocketMinimal<TReq>
    {        
        event Action<TAck, int> ReadCompleted; 
        event Action<int> WriteCompleted;
    }

    public interface IDragonSocket<T> : IDragonSocketMinimal<T> where T : IMessage 
    {        
        event Action<T> ReadCompleted;
        event Action WriteCompleted;
    }

    public interface IDragonSocketMinimal<in TReq> : IDisposable
    {
        event EventHandler<SocketAsyncEventArgs> OnDisconnected;
        SocketState State { get; set; }
        IPEndPoint RemoteEndPoint { get; }
        IPEndPoint LocalEndPoint { get; }
        void Send(TReq message);
        void Activate();
        /// <summary>
        /// For reuse, Socket and eventargs are not disposed.
        /// </summary>
        void Disconnect(SocketAsyncEventArgs e = null);
    }
}