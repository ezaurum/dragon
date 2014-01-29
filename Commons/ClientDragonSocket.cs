using System;
using System.Net;
using System.Net.Sockets;

namespace Dragon
{
    #region one type message
    /// <summary>
    ///     Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientDragonSocket<T> : DragonSocket<T> where T : IMessage
    {
        private readonly Connector _connector;
        public event EventHandler<SocketAsyncEventArgs> ConnectFailed
        {
            add { _connector.ConnectFailed += value; }
            remove { _connector.ConnectFailed -= value; }
        }

        public event EventHandler<SocketAsyncEventArgs> ConnectSuccess
        {
            add { _connector.ConnectSuccess += value; }
            remove { _connector.ConnectSuccess -= value; }
        }

        public void Connect(IPEndPoint endPoint)
        {
            _connector.Connect(endPoint);
            Socket = _connector.Socket;
        }

        public void Connect(string ipAddress, int port)
        {
            _connector.Connect(ipAddress, port);
            Socket = _connector.Socket;
        }
        
        public ClientDragonSocket(IMessageFactory<T> factory)
            : base(factory)
        {
            _connector = new Connector();
            ConnectSuccess += (sender, args) => Activate();
        }
    }

    #endregion
    
    #region two type message
    /// <summary>
    /// Client Socket. 
    /// Has Request, Acknowlege templates
    /// </summary>
    public class ClientDragonSocket<TReq, TAck> : DragonSocket<TReq, TAck>
    {
        private readonly Connector _connector;
        public event EventHandler<SocketAsyncEventArgs> ConnectFailed
        {
            add { _connector.ConnectFailed += value; }
            remove { _connector.ConnectFailed -= value; }
        }

        public event EventHandler<SocketAsyncEventArgs> ConnectSuccess
        {
            add { _connector.ConnectSuccess += value; }
            remove { _connector.ConnectSuccess -= value; }
        }

        public void Connect(IPEndPoint endPoint)
        {
            _connector.Connect(endPoint);
            Socket = _connector.Socket;
        }

        public void Connect(string ipAddress, int port)
        {
            _connector.Connect(ipAddress, port);
        }

        public ClientDragonSocket(IMessageFactory<TReq, TAck> factory, byte[] buffer, int index, int length)
            : base(factory, buffer, index, length)
        {
            _connector = new Connector();
            ConnectSuccess += (sender, args) => Activate();
        }
    }
    #endregion
}