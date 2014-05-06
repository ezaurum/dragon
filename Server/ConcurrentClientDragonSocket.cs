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
        private readonly bool _activateMessageEnable;

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
            IMessageConverter<TReq, TAck> converter, TReq acitvateMessage, bool autoReconnect = true)
            : base(converter)
        {
            _activateMessageEnable = true;
            _acitvateMessage = acitvateMessage;
            _connector = new Connector(0);
            ConnectSuccess += ActivateOnConnectSuccess;
            if (autoReconnect) Disconnected += _connector.Reconnect;
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
            //just send first in block
            byte[] result;
            int code;
            _converter.GetByte(message, out result, out code);
            try
            {
                Socket.Send(result);
            }
            catch (Exception e)
            {
                //if error
                Disconnect();
                return;
            }
            
            ContinueSendingIfExist();
        }

        private void ContinueSendingIfExist()
        {
            SendAsyncFromQueue();
        }
    }
}