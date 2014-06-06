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
    #region one type message

    /// <summary>
    ///     Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientDragonSocket<T> : ClientDragonSocket<T, T>
    {
        public ClientDragonSocket(IMessageConverter<T, T> converter)
            : base(converter)
        {
        }

        public ClientDragonSocket(IMessageConverter<T, T> converter,
            T hearbeatMessage, int interval) : base(converter, hearbeatMessage, interval)
        {
            
        }
    }

    #endregion

    #region two type message

    /// <summary>
    ///     Client Socket.
    ///     Has Request, Acknowlege templates
    /// </summary>
    public class ClientDragonSocket<TReq, TAck> : DragonSocket<TReq, TAck>,
        IConnectable, IBeatable<TReq>
    {
        #region connection
        private readonly Timer _connectTimer;

        public const int DefaultRetryLimit = 10;
        public const int DefaultInterval = 1500;

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
                return;
            }
            _connectTimer.Stop();
            // timer set
            RetryCount = 0;

            Activate();

            if (HeartBeatEnable) _heartBeatMaker.Start();

            if (null != ConnectSuccess) ConnectSuccess(sender, e);

            Interlocked.Exchange(ref _connecting, 0);
        }

        public EndPoint IpEndpoint
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
            if (Interlocked.CompareExchange(ref _connecting, 1,0) > 0) return;
            if (_connectTimer.Enabled) return;
            
            InitSocket();
            _connectTimer.Start();
            ConnectAsync();
        }

        private void ConnectAsync()
        {
            if(!Socket.ConnectAsync(_connectEventArgs))
                DefaultConnectCompleted(null, _connectEventArgs);
        }

        public void Reconnect(object sender, SocketAsyncEventArgs e)
        {
           Connect(); 
        }

        #endregion

        public event Action<TAck, int> ReadCompleted
        {
            add { Converter.MessageConverted += value; }
            remove { Converter.MessageConverted -= value; }
        }

        public ClientDragonSocket(IMessageConverter<TReq, TAck> converter)
            : base(converter)
        {
            _connectEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = IpEndpoint };
            _connectEventArgs.Completed += DefaultConnectCompleted;

            //ip endpoint set _connect event args property
            IpEndpoint = EndPointStorage.DefaultDestination;

            RetryLimit = DefaultRetryLimit;
            _connectTimer = new Timer {Interval = DefaultInterval, AutoReset = true};
            _connectTimer.Elapsed += CheckReconnect;
        }

        private readonly HeartBeatMaker<TReq> _heartBeatMaker;

        public bool HeartBeatEnable { get; set; }
        public TReq HeartBeatMessage { get; set; }
        
        public event Action<TReq> UpdateMessage
        {
            add { _heartBeatMaker.UpdateMessage += value; }
            remove { _heartBeatMaker.UpdateMessage -= value; }
        }

        public ClientDragonSocket(IMessageConverter<TReq, TAck> converter,
            TReq beatMessage, int interval = 750) : this(converter)
        {
            HeartBeatEnable = true;
            HeartBeatMessage = beatMessage;
            _heartBeatMaker = new HeartBeatMaker<TReq>(this, HeartBeatMessage,
                interval);
            Disconnected += _heartBeatMaker.Stop;
        }
    }

    #endregion
}