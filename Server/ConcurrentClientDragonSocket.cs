using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Dragon
{
    /// <summary>
    ///     Client Socket. auto reconnect, concorrent
    ///     Has Request, Acknowlege templates
    /// </summary>
    public class ConcurrentClientDragonSocket<TReq, TAck> :
        ConcurrentDragonSocket<TReq, TAck>,
        IConnectable
    {
        private readonly TReq _acitvateMessage;
        private readonly bool _activateMessageEnable;

        public ConcurrentClientDragonSocket(
            IMessageConverter<TReq, TAck> converter, TReq acitvateMessage, bool autoReconnect = true)
            : base(converter)
        {
            _activateMessageEnable = true;
            _acitvateMessage = acitvateMessage;

            _connectEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = IpEndpoint };
            _connectEventArgs.Completed += DefaultConnectCompleted;

            //ip endpoint set _connect event args property
            IpEndpoint = EndPointStorage.DefaultDestination;

            _connectTimer = new Timer {Interval = 1500, AutoReset = true};
            _connectTimer.Elapsed += CheckReconnect;

            if (autoReconnect)
                Disconnected += Reconnect;
        } 

        private void Activate(TReq message)
        {
            Activate();
            //just send first in block
            byte[] result;
            int code;
            Converter.GetByte(message, out result, out code);
            try
            {
                Socket.Send(result);
            }
            catch (Exception e)
            {
                //if error
                Disconnect();
                return;
            }
            
            ContinueSendingIfExist();
        }

        private void ContinueSendingIfExist()
        {
            SendAsyncFromQueue();
        } 
        
        private readonly Timer _connectTimer;

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
            OffState(SocketState.Connectiong);

            if (e.SocketError != SocketError.Success)
            { 
                return;
            }
            OnState(SocketState.Connected);
            _connectTimer.Stop();
            // timer set
            RetryCount = 0;

            //activate on connection successs
            if (_activateMessageEnable)
            {
                Activate(_acitvateMessage);
            }
            else
            {
                Activate();
            }
            
            ConnectSuccess(sender, e);
            
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

        public event EventHandler<SocketAsyncEventArgs> ConnectFailed;
        public event EventHandler<SocketAsyncEventArgs> ConnectSuccess;

        /// <summary>
        ///     Default handler for connect timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckReconnect(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("retry : {0}",RetryCount);
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
            if (IsState(SocketState.Connectiong | SocketState.Connected))
                return;

            if (!OnState(SocketState.Connectiong))
                return;

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

        private void Reconnect(object sender, SocketAsyncEventArgs e)
        {
            Connect(); 
        }
    }
}