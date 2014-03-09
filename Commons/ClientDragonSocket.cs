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
    public class ClientDragonSocket<T> : ClientDragonSocket<T, T>
    {
        public ClientDragonSocket(IMessageConverter<T, T> converter)
            : base(converter)
        {
        }

        public ClientDragonSocket(IMessageConverter<T, T> converter,
            T hearbeatMessage) : base(converter, hearbeatMessage)
        {
            
        }
    }

    #endregion

    #region two type message

    /// <summary>
    ///     Client Socket.
    ///     Has Request, Acknowlege templates
    /// </summary>
    public class ClientDragonSocket<TReq, TAck> : DragonSocket<TReq, TAck>,
        IConnectable, IBeatable<TReq>
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
        }

        public void Connect(string ipAddress, int port)
        {
            _connector.Connect(ipAddress, port);
        }

        public ClientDragonSocket(IMessageConverter<TReq, TAck> converter)
            : base(converter)
        {
            _connector = new Connector();
            Socket = _connector.Socket;
            ConnectSuccess += ActivateOnConnectSuccess;
        }

        private readonly HeartBeatMaker<TReq> _heartBeatMaker;

        public bool HeartBeatEnable { get; set; }
        public TReq HeartBeatMessage { get; set; }
        
        public event Action<TReq> UpdateMessage
        {
            add { _heartBeatMaker.UpdateMessage += value; }
            remove { _heartBeatMaker.UpdateMessage -= value; }
        }
    
        public ClientDragonSocket(IMessageConverter<TReq, TAck> converter,
            TReq beatMessage) : this(converter)
        {
            HeartBeatEnable = true;
            HeartBeatMessage = beatMessage;
            _heartBeatMaker = new HeartBeatMaker<TReq>(this, HeartBeatMessage);
            Disconnected += _heartBeatMaker.Stop;
            ConnectSuccess += _heartBeatMaker.Start;
        }

        private void ActivateOnConnectSuccess(object sender,
            SocketAsyncEventArgs e)
        {
            Activate();

            if (!HeartBeatEnable) return;
            _heartBeatMaker.Start();
        }
    }

    #endregion
}