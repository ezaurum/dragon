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

        public ClientDragonSocket(IMessageFactory<T> factory)
            : base(factory)
        {
            Socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 1000,
                ReceiveTimeout = 1000
            };

            RetryInterval = 500;
            RetryLimit = 5;
            _connectTimer = new Timer {Interval = RetryInterval};
            _connectTimer.Elapsed += CheckReconnect;

            IpEndpoint = new IPEndPoint(DefaultConnectIpAddresss, DefaultListeningPortNumber);
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
    }
}