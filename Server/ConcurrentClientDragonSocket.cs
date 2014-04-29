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
        private readonly TReq _acitvateMessage;
        private bool _activateMessageEnable;

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
            IMessageConverter<TReq, TAck> converter, TReq acitvateMessage)
            : base(converter)
        {
            _activateMessageEnable = true;
            _acitvateMessage = acitvateMessage;
            _connector = new Connector(0);
            ConnectSuccess += ActivateOnConnectSuccess;
        } 

        private void ActivateOnConnectSuccess(object sender,
            SocketAsyncEventArgs e)
        {
            Socket = _connector.Socket;

            if (_activateMessageEnable)
            {
                Activate(_acitvateMessage);
            }
            else
            {
                Activate();
            }

        }

        private void Activate(TReq message)
        {
            Activate();

            //just send first
            SendAsync(message);
        }
    }
}