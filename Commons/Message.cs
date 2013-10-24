using System;
using System.Net.Sockets;

namespace Dragon.Message
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

namespace Dragon
{
    public delegate void SocketAsyncEventHandler(object sender, SocketAsyncEventArgs e);

    public interface ITokenProvider
    {
        IAsyncUserToken NewAsyncUserToken();
    }

    public interface IAsyncUserToken : IDisposable
    {
        Socket Socket { get; set; }
        SocketAsyncEventArgs ReadArgs { get; set; }
        SocketAsyncEventArgs WriteArgs { get; set; }
        INetworkManager NetworkManager { get; set; }
        bool IsDisposed { get; set; }
        void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred);
    }

    public interface INetworkManager
    {
        ITokenProvider TokenProvider { get; set; }
        void SendBytes(Socket socket, SocketAsyncEventArgs e);
    }
}