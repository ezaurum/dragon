using System;
using System.Net;
using System.Net.Sockets;

namespace Dragon
{
    public interface IConnectable
    {
        event EventHandler<SocketAsyncEventArgs> ConnectFailed;
        event EventHandler<SocketAsyncEventArgs> ConnectSuccess;
        void Connect(IPEndPoint endPoint);
        void Connect(string ipAddress, int port);
        void Connect();
    }
}