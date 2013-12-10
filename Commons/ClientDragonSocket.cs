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
        private const int DefaultListeningPortNumber = 10008;
        private static readonly IPAddress DefaultConnectIpAddresss = IPAddress.Loopback;
        private readonly Timer _connectTimer;
        private int _retryCount;
        private readonly bool _heartbeat;
        private readonly SocketAsyncEventArgs _heartbeatEventArgs;
        private Timer _heartbeatTimer;

        public override void Disconnect()
        {
            if (null != _heartbeatEventArgs) _heartbeatEventArgs.Dispose();
            base.Disconnect();
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
            Socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            _heartbeat = heartbeat;

            RetryInterval = 1500;
            RetryLimit = 5;
            _connectTimer = new Timer {Interval = RetryInterval,AutoReset = false};
            _connectTimer.Elapsed += CheckReconnect;

            IpEndpoint = new IPEndPoint(DefaultConnectIpAddresss, DefaultListeningPortNumber);


            //set heartbeats
            if (!_heartbeat) return;
            _heartbeatEventArgs = new SocketAsyncEventArgs();
            _heartbeatEventArgs.Completed += OnHeartbeat;
            byte[] heartbeatBuffer = BitConverter.GetBytes((Int16)sizeof(Int16));
            _heartbeatEventArgs.SetBuffer(heartbeatBuffer, 0, sizeof(Int16));
            _heartbeatTimer = new Timer { Interval = 500 };
            _heartbeatTimer.Elapsed += Beat;
        }

        private void InitConnectEventArg()
        {
            ConnectEventArgs = new SocketAsyncEventArgs {RemoteEndPoint = IpEndpoint};
            ConnectEventArgs.Completed += StopTimerOnConnected;
            ConnectEventArgs.Completed += DefaultConnectSuccess;
        }

        /// <summary>
        /// Default Event handler for connection success.
        /// Activate socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefaultConnectSuccess(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            ConnectSuccess(sender, e);
            Activate();
        }


        public EndPoint IpEndpoint { get; set; }
        public int RetryInterval { get; set; }
        public int RetryLimit { get; set; }
        private SocketAsyncEventArgs ConnectEventArgs { get; set; }
        
        public event EventHandler<SocketAsyncEventArgs> ConnectFailed;
        public event EventHandler<SocketAsyncEventArgs> ConnectSuccess;


        /// <summary>
        /// Default handler for connect timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckReconnect(object sender, ElapsedEventArgs e)
        {
            if (ConnectEventArgs.SocketError == SocketError.Success) return;

            if (_retryCount < RetryLimit)
            {
                _retryCount++;
                Connect();
            }
            else if (ConnectFailed != null)
            {
                ConnectFailed(sender, ConnectEventArgs);
            }
        }

        /// <summary>
        /// Default event handler for connected event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="socketAsyncEventArgs"></param>
        private void StopTimerOnConnected(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (ConnectEventArgs.SocketError == SocketError.Success)
            {
                _connectTimer.Stop();
            }
        }

        public void Connect(string ipAddress, int port)
        {
            IpEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            InitConnectEventArg();
            Connect();
        }

        private void Connect()
        {
            // timer set
            _connectTimer.Start();
            if (!Socket.ConnectAsync(ConnectEventArgs))
            {
                StopTimerOnConnected(null, ConnectEventArgs);
            }
        }

        private void OnHeartbeat(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Disconnect();
            }
            Console.WriteLine("pitapat pitapat");
        }
    }
}