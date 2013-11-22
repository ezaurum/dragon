using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Dragon.Client
{
    public class SocketConnector
    {
        private enum ConnectorState
        {
            BeforeInitialized
        }
        private Socket _socket;
        private readonly EndPoint _ipEndpoint;

        private const int DefaultListeningPortNumber = 10008;
        public int RetryInterval { get; set; }
        public int RetryLimit { get; set; }
        private static readonly IPAddress DefaultConnectIpAddresss= IPAddress.Loopback;
        private static readonly ILogger Logger = LoggerManager.GetLogger(typeof(SocketConnector));

        private SocketAsyncEventArgs ConnectEventArgs { get; set; }
        
        private ConnectorState _state = ConnectorState.BeforeInitialized;
        private int _retryCount;

        public SocketConnector()
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            ConnectEventArgs = new SocketAsyncEventArgs();

            _ipEndpoint = new IPEndPoint(DefaultConnectIpAddresss, DefaultListeningPortNumber);

            ConnectEventArgs.RemoteEndPoint = _ipEndpoint;

            ConnectEventArgs.Completed += ConnectEventArgsOnCompleted;
            
            RetryInterval = 500;
            RetryLimit = 5;
        }

        private void ConnectEventArgsOnCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                if (_retryCount < RetryLimit)
                {
                    Logger.Debug("Connectiton Failed. Because of {0}. Try reonnect {1}/{2} after {3}ms", socketAsyncEventArgs.SocketError, _retryCount, RetryLimit, RetryInterval);
                    _retryCount++;
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
        
        public SocketConnector(string ipAddress, int port) : this()
        {
            _ipEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        public void Connect()
        {
            if (!_socket.ConnectAsync(ConnectEventArgs))
            {
                ConnectEventArgsOnCompleted(null, ConnectEventArgs);
            }
        }
    }
}