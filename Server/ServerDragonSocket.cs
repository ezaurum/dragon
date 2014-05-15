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

    public class ServerDragonSocket<TReq, TAck> : ConcurrentDragonSocket<TReq, TAck>
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

            Converter.MessageConverted += DefaultReadComplete;
        }

        private void DefaultReadComplete(TAck arg1, int arg2)
        {
            if (!HeartbeatEnable || !_heartBeatReceiver.IsHeartBeat(arg1))
                return;
            _heartBeatReceiver.Receive(arg1, arg2);
        }
        
        private HeartBeatReceiver<TAck> _heartBeatReceiver;

        public bool HeartbeatEnable { get; set; }

        public event Action<TAck, int> ReceiveHeartbeat
        {
            add { _heartBeatReceiver.ReceiveHeartbeat += value; }
            remove { _heartBeatReceiver.ReceiveHeartbeat -= value; }
        }

        public HeartBeatReceiver<TAck> HeartBeatReceiver
        {
            get { return _heartBeatReceiver; }
            set
            {
                if (null != _heartBeatReceiver)
                    _heartBeatReceiver.OnBeatStop -= Disconnect;

                _heartBeatReceiver = value;
                _heartBeatReceiver.OnBeatStop += Disconnect;
            }
        }

        private void Disconnect()
        {
            base.Disconnect();
        }

        public event EventHandler<SocketAsyncEventArgs> Accepted; 
    }
}