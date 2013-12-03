using System;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    /// Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServerDragonSocket<T> : DragonSocket<T> where T : IMessage
    {
        public ServerDragonSocket(Socket acceptSocket, IMessageFactory<T> factory) : base(factory)
        {
            Socket = acceptSocket;

            if (null != Accepted)
                Accepted(Socket, null);
           
            //start to read
            ReadRepeat();
        }
        
        public event EventHandler<SocketAsyncEventArgs> Accepted;
    }
}