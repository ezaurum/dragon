using System;
using System.Net;
using System.Net.Sockets;

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
        private static readonly IPAddress DefaultConnectIpAddresss= IPAddress.Loopback;
        private static readonly ILogger Logger = LoggerManager.GetLogger(typeof(SocketConnector));

        private SocketAsyncEventArgs ConnectEventArgs { get; set; }
        
        private ConnectorState _state = ConnectorState.BeforeInitialized;

        public SocketConnector()
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            ConnectEventArgs = new SocketAsyncEventArgs();

            _ipEndpoint = new IPEndPoint(DefaultConnectIpAddresss, DefaultListeningPortNumber);

            ConnectEventArgs.RemoteEndPoint = _ipEndpoint;

            ConnectEventArgs.Completed += ConnectEventArgsOnCompleted;
        }

        private void ConnectEventArgsOnCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Logger.Debug("Connected.");
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