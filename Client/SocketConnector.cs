using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Dragon.Client
{
    public class SocketConnector
    {
        private const int DefaultListeningPortNumber = 10008;
        private static readonly IPAddress DefaultConnectIpAddresss = IPAddress.Loopback;
        private static readonly ILogger Logger = LoggerManager.GetLogger(typeof (SocketConnector));

        private int _retryCount;
        private ConnectorState _state = ConnectorState.BeforeInitialized;

        private Timer _firstConnectTimer;

        public SocketConnector()
        {
            Socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            _firstConnectTimer = new Timer();
            _firstConnectTimer.Interval = 2000;
            _firstConnectTimer.Elapsed += (sender, e) =>
            {
                Logger.Debug("connection in first time {0}", ConnectEventArgs.SocketError);

                if (ConnectEventArgs.SocketError != SocketError.Success)
                {
                    Connect();
                }
                else
                {
                    _firstConnectTimer.Stop();
                }
            };

            ConnectEventArgs = new SocketAsyncEventArgs();

            ConnectEventArgs.Completed += ConnectEventArgsOnCompleted;

            RetryInterval = 500;
            RetryLimit = 5;
            Socket.SendTimeout = 1000;
            Socket.ReceiveTimeout = 1000;
            IpEndpoint = new IPEndPoint(DefaultConnectIpAddresss, DefaultListeningPortNumber);
            ConnectEventArgs.RemoteEndPoint = IpEndpoint;
        }

        public Socket Socket { get; set; }

        public EndPoint IpEndpoint { get; set; }
        public int RetryInterval { get; set; }
        public int RetryLimit { get; set; }
        public SocketAsyncEventArgs ConnectEventArgs { get; set; }

        private void ConnectEventArgsOnCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            _retryCount++;
            _firstConnectTimer.Stop();
            if (socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                if (_retryCount < RetryLimit)
                {
                    Logger.Debug("Connectiton Failed. Because of {0}. Try reonnect {1}/{2} after {3}ms",
                        socketAsyncEventArgs.SocketError, _retryCount, RetryLimit, RetryInterval);
                    Thread.Sleep(RetryInterval);
                    Connect();
                }
                else
                {
                    Logger.Fatal("Connectiton Failed. {0}", socketAsyncEventArgs.SocketError);
                }
            }
            else
            {
                Logger.Debug("Connected.");
            }
        }

        private void Connect()
        {
            _firstConnectTimer.Start();
            if (!Socket.ConnectAsync(ConnectEventArgs))
            {
                ConnectEventArgsOnCompleted(null, ConnectEventArgs);
            }
        }

        public void Connect(string ipAddress, int port)
        {
            IpEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            ConnectEventArgs.RemoteEndPoint = IpEndpoint;
            Connect();
        }

        private enum ConnectorState
        {
            BeforeInitialized
        }
    }
}