﻿using System;
using System.Net.Sockets;

namespace Dragon.Message
{
    public interface IGameMessage
    {
        Int16 Length { get; }
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes);
        DateTime PacketTime { get; set; }
    }

    public interface IMessageProcessor<T> where T : IGameMessage
    {
        T ReceivedMessage { get; set; }
        T SendingMessage { set; }
    }
}

namespace Dragon
{
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


    public interface ISessionManager
    {
        void Login(object sender, SocketAsyncEventArgs e);
    }

    public interface IEventArgsPool<T> where T : EventArgs
    {
        void Push(T item);
        T Pop();
        event EventHandler<T> Completed;
        void Prepare(int capacity);
    }
}

namespace Dragon.Session
{
    public class GameSession : IDisposable
    {
        public Guid Id { get; set; }
        public virtual void Dispose()
        {
            Id = Guid.Empty;
        }
    }
}