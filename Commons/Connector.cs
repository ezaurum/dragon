using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Dragon
{
    /// <summary>
    ///     Connect class wrapping socket
    /// </summary>
    public class Connector
    {
        private readonly Timer _connectTimer;
        public Socket Socket { get; private set; }

        public Connector()
        {
            _connectEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = IpEndpoint };
            _connectEventArgs.Completed += DefaultConnectCompleted;

            //ip endpoint set _connect event args property
            IpEndpoint = EndPointStorage.DefaultDestination;

            RetryLimit = 10;
            _connectTimer = new Timer {Interval = 1500, AutoReset = true};
            _connectTimer.Elapsed += CheckReconnect;
        }

        public Connector(IPEndPoint ipEndPoint) : this()
        {
            IpEndpoint = ipEndPoint;
        }

        public Connector(int retryLimit) : this()
        {
            RetryLimit = retryLimit;
        }

        public Connector(IPEndPoint ipEndPoint, int retryInterval, int retryLimit) : this(ipEndPoint)
        {
            _connectTimer.Interval = retryInterval;
            RetryLimit = retryLimit;
        }

        private void InitSocket()
        {
            Socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        }

        /// <summary>
        ///     Default Event handler for connection success.
        ///     Activate socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefaultConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Interlocked.CompareExchange(ref _connecting, 1, 2);
                return;
            }
            _connectTimer.Stop();
            // timer set
            RetryCount = 0;
            
            ConnectSuccess(sender, e);

            Interlocked.Exchange(ref _connecting, 0);
        }

        private EndPoint IpEndpoint
        {
            get { return _ipEndpoint; }
            set
            {
                _ipEndpoint = value;
                _connectEventArgs.RemoteEndPoint = value;
            }
        }

        public int RetryLimit { get; set; }
        public int RetryCount { get; private set; }
        
        private readonly SocketAsyncEventArgs _connectEventArgs;
        private EndPoint _ipEndpoint;
        private int _connecting;

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
            if (_connectEventArgs.SocketError != SocketError.Success && (0 == RetryLimit || RetryCount < RetryLimit))
            {
                RetryCount++;
                ConnectAsync();
                return;
            }

            //connection retry exceed retry limit
            _connectTimer.Stop();

            if (_connectEventArgs.SocketError == SocketError.Success || ConnectFailed == null) return;
            
            ConnectFailed(sender, _connectEventArgs);
            Interlocked.Exchange(ref _connecting, 0);
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

        public void Connect()
        {
            if (Interlocked.Exchange(ref _connecting, 1) == 1) return;
            if (_connectTimer.Enabled) return;
            
            InitSocket();
            _connectTimer.Start();
            ConnectAsync();
        }

        private void ConnectAsync()
        {
            if (Interlocked.CompareExchange(ref _connecting, 2, 1) == 2) return;
            if(!Socket.ConnectAsync(_connectEventArgs))
                DefaultConnectCompleted(null, _connectEventArgs);
        }

        public void Reconnect(object sender, SocketAsyncEventArgs e)
        {
           Connect(); 
        }
    }
}