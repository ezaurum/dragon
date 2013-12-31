using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Dragon
{
    /// <summary>
    ///     Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientDragonSocket<T> : DragonSocket<T> where T : IMessage
    {
        private readonly bool _heartbeat;
        private readonly SocketAsyncEventArgs _heartbeatEventArgs;
        private readonly Timer _heartbeatTimer;

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

        public override void Dispose()
        {
            if (null != _heartbeatEventArgs) _heartbeatEventArgs.Dispose();
            base.Dispose();
        }

        public override void Activate()
        {
            base.Activate();
            if (_heartbeat)
            {
                _heartbeatTimer.Start();
            }
        }

        private void Beat(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _heartbeatEventArgs.SetBuffer(0, sizeof(Int16));
            if (!Socket.SendAsync(_heartbeatEventArgs))
            {
                OnHeartbeat(Socket, _heartbeatEventArgs);
            }
        }

        public ClientDragonSocket(IMessageFactory<T> factory, bool heartbeat)
            : base(factory)
        {
            _connector = new Connector();
            _connector.ConnectSuccess += (sender, args) => Activate();
            _heartbeat = heartbeat;

            //set heartbeats
            if (!_heartbeat) return;
            _heartbeatEventArgs = new SocketAsyncEventArgs();
            _heartbeatEventArgs.Completed += OnHeartbeat;
            byte[] heartbeatBuffer = BitConverter.GetBytes((Int16)sizeof(Int16));
            _heartbeatEventArgs.SetBuffer(heartbeatBuffer, 0, sizeof(Int16));
            _heartbeatTimer = new Timer { Interval = 500 };
            _heartbeatTimer.Elapsed += Beat;
        }
        
        private void OnHeartbeat(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success) return;
            _heartbeatTimer.Stop();
            Disconnect();
        }
    }
}