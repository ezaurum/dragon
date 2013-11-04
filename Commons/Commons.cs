using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Dragon.Message
{
    public interface IGameMessage
    {
        Int16 Length { get; }
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes);
    }

    public interface IMessageProcessor<T> where T : IGameMessage
    {
        T ReceivedMessage { get; set; }
        T SendingMessage { set; }
        void ResetMessages();
    }
}

namespace Dragon
{
    public delegate void SocketAsyncEventHandler(object sender, SocketAsyncEventArgs e);

    public interface IRajaProvider
    {
        IRaja NewInstance();
    }

    public interface IRaja : IDisposable
    {
        Socket Socket { get; set; }
        SocketAsyncEventArgs ReadArgs { get; set; }
        SocketAsyncEventArgs WriteArgs { get; set; }
        INetworkManager NetworkManager { get; set; }
        bool IsDisposed { get; set; }
        bool AbleToSend { get; set; }
        void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred);
    }

    public interface INetworkManager
    {
        IRajaProvider RajaProvider { get; set; }
        void SendBytes(Socket socket, SocketAsyncEventArgs e);
    }
}

namespace Dragon.Session
{
    public class Session : IDisposable
    {
        public Guid Id { get; set; }
        public virtual void Dispose()
        {
            Id = Guid.Empty;
        }
    }
}