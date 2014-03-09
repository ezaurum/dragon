using System;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    /// Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServerDragonSocket<T> : ServerDragonSocket<T, T>
    { 
        public ServerDragonSocket(Socket acceptSocket, IMessageConverter<T, T> converter) : base(acceptSocket,converter)
        {
         
        }
    }

    public class ServerDragonSocket<TReq, TAck> : DragonSocket<TReq, TAck>
    {
        public ServerDragonSocket(Socket acceptSocket, IMessageConverter<TReq, TAck> converter)
            : base(converter)
        {
            Socket = acceptSocket;
            Socket.NoDelay = false;

            if (null != Accepted)
            {
                Accepted(Socket, null);
            }
        }

        public event EventHandler<SocketAsyncEventArgs> Accepted;
    }
}