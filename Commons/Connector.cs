using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Timers;

namespace Dragon
{
    /// <summary>
    ///     Connect class wrapping socket
    /// </summary>
    public class Connector
    {
        private readonly Timer _connectTimer;
        private Socket _socket;
        public Socket Socket
        {
            get { return _socket; }
        }
        
        public Connector()
        {
            IpEndpoint = EndPointStorage.DefaultDestination;
            RetryLimit = 10;
            _connectTimer = new Timer {Interval = 1500, AutoReset = true};
            _connectTimer.Elapsed += CheckReconnect;
        }

        public Connector(IPEndPoint ipEndPoint) : this()
        {
            IpEndpoint = ipEndPoint;
        }

        public Connector(IPEndPoint ipEndPoint, int retryInterval, int retryLimit) : this(ipEndPoint)
        {
            _connectTimer.Interval = retryInterval;
            RetryLimit = retryLimit;
        }

        private void InitConnectEventArg()
        {
            _socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

            _connectEventArgs = new SocketAsyncEventArgs {RemoteEndPoint = IpEndpoint};
            _connectEventArgs.Completed += DefaultConnectCompleted;
        }

        /// <summary>
        ///     Default Event handler for connection success.
        ///     Activate socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefaultConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("Default connect : "+e.SocketError);
            if (e.SocketError != SocketError.Success) return;
            
            ConnectSuccess(sender, e);
            _connectTimer.Stop();
            _connectEventArgs.Dispose();
        }

        private EndPoint IpEndpoint { get; set; }

        public int RetryLimit { get; set; }
        public int RetryCount { get; private set; }
        public double RetryInterval
        {
            get { return _connectTimer.Interval; }
            set { _connectTimer.Interval = value; }
        }

        private SocketAsyncEventArgs _connectEventArgs;

        public event EventHandler<SocketAsyncEventArgs> ConnectFailed;
        public event EventHandler<SocketAsyncEventArgs> ConnectSuccess;

        /// <summary>
        ///     Default handler for connect timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckReconnect(object sender, ElapsedEventArgs e)
        {
            //failed. retry
            if (_connectEventArgs.SocketError != SocketError.Success && RetryCount < RetryLimit)
            {
                RetryCount++;
                ConnectAsync();
                return;
            }

            //connection retry exceed retry limit
            _connectTimer.Stop();

            if (_connectEventArgs.SocketError == SocketError.Success || ConnectFailed == null) return;
            _connectEventArgs.Dispose();
            ConnectFailed(sender, _connectEventArgs);
        }

        public void Connect(IPEndPoint endPoint)
        {
            IpEndpoint = endPoint;
            Connect();
        }

        public void Connect(string ipAddress, int port)
        {
            if (Regex.IsMatch(ipAddress, @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}"))
            {
                IpEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            }
            else if (Dns.GetHostAddresses(ipAddress).Length > 0)
            {
                IpEndpoint = new IPEndPoint(Dns.GetHostAddresses(ipAddress)[0], port);
            }
            else
            {
                throw new InvalidDataException(string.Format("address {0} is not suitable.", ipAddress));
            }

            Connect();
        }

        private void Connect()
        { 
            InitConnectEventArg();

            // timer set
            RetryCount = 0;
            _connectTimer.Start(); 
            ConnectAsync();
        }

        private void ConnectAsync()
        {
            if (_socket.ConnectAsync(_connectEventArgs)) return;
            DefaultConnectCompleted(null, _connectEventArgs);
        }
    }
}