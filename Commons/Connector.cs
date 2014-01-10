using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Dragon
{
    /// <summary>
    /// Connect class wrapping socket
    /// </summary>
    public class Connector  
    {
        private readonly Timer _connectTimer;
        private int _retryCount;

        private readonly Socket _socket;

        public Socket Socket
        {
            get { return _socket; }
        }

        public Connector()
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            IpEndpoint = EndPointStorage.DefaultDestination; 
            _retryLimit = 10;
            _connectTimer = new Timer { Interval = 1500, AutoReset = true };
            _connectTimer.Elapsed += CheckReconnect;
        }

        public Connector(IPEndPoint ipEndPoint) : this()
        {
            IpEndpoint = ipEndPoint;
        }

        public Connector(IPEndPoint ipEndPoint, int retryInterval, int retryLimit) : this(ipEndPoint)
        {
            _connectTimer.Interval = retryInterval;
            _retryLimit = retryLimit;
        }

        private void InitConnectEventArg()
        {
            _connectEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = IpEndpoint };
            _connectEventArgs.Completed += DefaultConnectSuccess;
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
            _connectTimer.Stop();
        }

        private EndPoint IpEndpoint { get; set; }
        private readonly int _retryLimit;
        private SocketAsyncEventArgs _connectEventArgs;

        public event EventHandler<SocketAsyncEventArgs> ConnectFailed;
        public event EventHandler<SocketAsyncEventArgs> ConnectSuccess;

        /// <summary>
        /// Default handler for connect timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckReconnect(object sender, ElapsedEventArgs e)
        {
            //failed. retry
            if (_connectEventArgs.SocketError != SocketError.Success && _retryCount < _retryLimit)
            {
                _retryCount++;
                ConnectAsync();
                return;
            }

            //connection retry exceed retry limit
            _connectTimer.Stop();

            if (_connectEventArgs.SocketError == SocketError.Success || ConnectFailed == null) return;
            ConnectFailed(sender, _connectEventArgs);
        }

        public void Connect(IPEndPoint endPoint)
        {
            IpEndpoint = endPoint;
            Connect();
        }

        public void Connect(string ipAddress, int port)
        {
            IpEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Connect();
        }

        private void Connect()
        {
            if (null == _connectEventArgs)
                InitConnectEventArg();

            // timer set
            _retryCount = 0;
            _connectTimer.Start();
            ConnectAsync();
        }

        private void ConnectAsync()
        {
            if (_socket.ConnectAsync(_connectEventArgs)) return;
            DefaultConnectSuccess(null, _connectEventArgs);
        }
        
    }
}