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
        private ClientSocketState _state = ClientSocketState.BeforeInitialized;

        public ClientDragonSocket()
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

            ConnectEventArgs = new SocketAsyncEventArgs();
            ConnectEventArgs.Completed += StopTimerOnConnected;

            IpEndpoint = new IPEndPoint(DefaultConnectIpAddresss, DefaultListeningPortNumber);
            ConnectEventArgs.RemoteEndPoint = IpEndpoint;
        }

        public EndPoint IpEndpoint { get; set; }
        public int RetryInterval { get; set; }
        public int RetryLimit { get; set; }
        private SocketAsyncEventArgs ConnectEventArgs { get; set; }

        public event MessageEventHandler<T> Connected;
        public event EventHandler<SocketAsyncEventArgs> ConnectFailed;

        public void Disconnect()
        {
            //TODO disconnect
        }

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
            ConnectEventArgs.RemoteEndPoint = IpEndpoint;
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


        private enum ClientSocketState
        {
            BeforeInitialized
        }
    }

    /// <summary>
    ///     Socket Wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DragonSocket<T> : IDragonSocket<T> where T : IMessage
    {
        private MessageConverter<T> _messageConverter;
        public event MessageEventHandler<T> ReadCompleted;
        public event MessageEventHandler<T> WriteCompleted;

        //TODO event disconnected
        //TODO event connected

        public Socket Socket { set; protected get; }
        public SocketAsyncEventArgs WriteEventArgs { set; protected get; }
        public SocketAsyncEventArgs ReadEventArgs { set; protected get; }

        public void Send(T message)
        {
            WriteEventArgs.SetBuffer(message.ToByteArray(), 0, message.Length);

            if (!Socket.SendAsync(WriteEventArgs))
            {
                //TODO
                WriteCompleted(message);
            }
        }

        public event MessageEventHandler<T> Disconnected;
    }
}