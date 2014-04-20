using System;
using System.Net;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    ///     Client Socket. auto reconnect, concorrent
    ///     Has Request, Acknowlege templates
    /// </summary>
    public class ConcurrentClientDragonSocket<TReq, TAck> :
        ConcurrentDragonSocket<TReq, TAck>,
        IConnectable
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

        public ConcurrentClientDragonSocket(
            IMessageConverter<TReq, TAck> converter)
            : base(converter)
        {
            _connector = new Connector(0);
            ConnectSuccess += ActivateOnConnectSuccess;
        }

        private HeartBeatMaker<TReq> _heartBeatMaker;

        public bool HeartBeatEnable { get; set; }
        public TReq HeartBeatMessage { get; set; }

        public event Action<TReq> UpdateMessage
        {
            add { _heartBeatMaker.UpdateMessage += value; }
            remove { _heartBeatMaker.UpdateMessage -= value; }
        }

        public ConcurrentClientDragonSocket(
            IMessageConverter<TReq, TAck> converter,
            TReq beatMessage, int interval = 750) : this(converter)
        {
            InitHeartBeatMaker(beatMessage, interval);
        }

        private void InitHeartBeatMaker(TReq beatMessage, int interval)
        {
            HeartBeatEnable = true;
            HeartBeatMessage = beatMessage;
            _heartBeatMaker = new HeartBeatMaker<TReq>(this, HeartBeatMessage,
                interval);

            Disconnected += _heartBeatMaker.Stop;
            ConnectSuccess += _heartBeatMaker.Start;
        }

        private void ActivateOnConnectSuccess(object sender,
            SocketAsyncEventArgs e)
        {
            Socket = _connector.Socket;

            Activate();

            if (!HeartBeatEnable) return;
            _heartBeatMaker.Start();
        }
    }
}