using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Dragon.Client
{
    public class SocketConnector
    {
        private const int DefaultListeningPortNumber = 10008;
        private static readonly IPAddress DefaultConnectIpAddresss = IPAddress.Loopback;
        private static readonly ILogger Logger = LoggerManager.GetLogger(typeof (SocketConnector));

        private int _retryCount;
        private ConnectorState _state = ConnectorState.BeforeInitialized;

        public SocketConnector()
        {
            Socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            ConnectEventArgs = new SocketAsyncEventArgs();

            ConnectEventArgs.Completed += ConnectEventArgsOnCompleted;

            RetryInterval = 500;
            RetryLimit = 5;

            IpEndpoint = new IPEndPoint(DefaultConnectIpAddresss, DefaultListeningPortNumber);
            ConnectEventArgs.RemoteEndPoint = IpEndpoint;
        }

        public Socket Socket { get; set; }

        public EndPoint IpEndpoint { get; set; }
        public int RetryInterval { get; set; }
        public int RetryLimit { get; set; }
        private SocketAsyncEventArgs ConnectEventArgs { get; set; }

        private void ConnectEventArgsOnCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                if (_retryCount < RetryLimit)
                {
                    Logger.Debug("Connectiton Failed. Because of {0}. Try reonnect {1}/{2} after {3}ms",
                        socketAsyncEventArgs.SocketError, _retryCount, RetryLimit, RetryInterval);
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

        private void Connect()
        {
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