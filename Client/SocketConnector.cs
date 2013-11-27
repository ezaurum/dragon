using System;
using System.Collections.Generic;
using System.Linq;
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


//TODO remove below
namespace Dragon.Client
{

    public interface IRajaProvider
    {
        IRaja NewInstance();
    }

    public interface IRaja : IDisposable
    {
        Socket Socket { get; set; }
        SocketAsyncEventArgs ReadArgs { get; set; }
        SocketAsyncEventArgs WriteArgs { get; set; }
        Unity3DNetworkManager NetworkManager { get; set; }
        bool IsDisposed { get; set; }
        bool AbleToSend { get; set; }
        void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred);
    }

    public class AsyncClientUserToken : IRaja 
    {   
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadArgs { get; set; }
        public SocketAsyncEventArgs WriteArgs { get; set; }
        public Unity3DNetworkManager NetworkManager { get; set; }
        public bool IsDisposed { get; set; }
        public bool AbleToSend { get; set; }

        public void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {

            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class SimpleAsyncClientUserToken : AsyncClientUserToken
    {
        public IMessage Message { get; set; }
    }

    public class QueueAsyncClientUserToken : AsyncClientUserToken
    {
        private readonly Queue<IMessage> _messages = new Queue<IMessage>();

        public QueueAsyncClientUserToken()
        {
            Buffer = new byte[1024];
        }

        public IMessage Message
        {   
            get
            {
                if (IsEmpty()) return null;
                return _messages.Dequeue();
            }
            set
            {
                if (_messages.Count > 0 && _messages.Last() != value)
                    _messages.Enqueue(value);
            }
        }

        public short MessageLength { get; set; }
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }

        public bool IsEmpty()
        {
            return _messages.Count < 1;
        }
    }

     public class ClientRajaProvider : IRajaProvider
    {
        public IRaja NewInstance()
        {
            return new QueueAsyncClientUserToken();
        }

        public IRaja NewWriteAsyncUserToken()
        {
            return new SimpleAsyncClientUserToken();
        }
    }

    public class Unity3DNetworkManager 
    {
        private readonly Socket _socket;
        
        private bool _started;
        private SocketAsyncEventArgs _connectEventArgs;
        private SocketAsyncEventArgs _readEventArgs; 
        private SocketAsyncEventArgs _writeEventArgs;
        private readonly EndPoint _ipEndpoint;
        private readonly object _lock = new object();

        public bool OnLine { get; set; }

        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageSend;
        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageReceive;
        public event EventHandler<SocketAsyncEventArgs> OnAfterConnectOnce;

        public Unity3DNetworkManager(string ipAddress, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            _ipEndpoint = new IPEndPoint( IPAddress.Parse(ipAddress), port);

            RajaProvider = new ClientRajaProvider();
        }
        
        public void SendMessage(IMessage message)
        {
            byte[] byteArray = message.ToByteArray();
            
            Console.WriteLine("send {0} bytes.", byteArray.Length);
            
            try
            {
                lock (_lock)
                {
                    _writeEventArgs.SetBuffer(byteArray, 0, byteArray.Length);
                    if (!_socket.SendAsync(_writeEventArgs))
                    {
                        Send_Completed(_writeEventArgs);
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                if (typeof (ObjectDisposedException) == e.GetType())
                {
                  //  Reconnect();
                }
                Thread.Sleep(1);
            }
            catch (SocketException e)
            {
                //Reconnect();
            }
        }

        private void Send_Completed(SocketAsyncEventArgs writeEventArgs)
        {
            writeEventArgs.SetBuffer(new byte[1024], 0, 1024);
            OnAfterMessageSend(this, writeEventArgs);
        }

        public void Start()
        {
            if (_started) return;

            Init();

            Connect();
        }

        private void Connect()
        {
            if (!_socket.ConnectAsync(_connectEventArgs))
            {
                Connect_Completed(this, _connectEventArgs);
            }
        }

        private void Init()
        {
            if (null == OnAfterMessageReceive)
            {
                throw new InvalidOperationException("OnAfterMessageReceive is not setted.");
            }
            _readEventArgs = new SocketAsyncEventArgs();
            _readEventArgs.Completed += Read_Completed;
            _readEventArgs.SetBuffer(new byte[1024], 0, 1024);
            

            if (null == OnAfterMessageSend)
            {
                throw new InvalidOperationException("OnAfterMessageSend is not setted.");
            }
            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += OnAfterMessageSend;

            //started
            _started = true;

            _connectEventArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = _ipEndpoint
            };
            _connectEventArgs.Completed += Connect_Completed;
        }

        private void Read_Completed(object sender, SocketAsyncEventArgs e)
        {
            while (true)
            {
                Console.WriteLine("READ_COMPLETED");
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    Console.WriteLine("Has Data {0}", e.BytesTransferred);
                    OnAfterMessageReceive(sender, e);
                    Console.WriteLine("Recursive READ");
                }
                else if (e.SocketError != SocketError.Success)
                {
                    OnLine = false;
                    return;
                }

                if (!_socket.ReceiveAsync(e))
                {
                    continue;
                }

                break;
            }
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if( null != OnAfterConnectOnce) OnAfterConnectOnce(sender, e);
                _readEventArgs.UserToken = RajaProvider.NewInstance();
                _writeEventArgs.UserToken = ((ClientRajaProvider)RajaProvider).NewWriteAsyncUserToken();
                
                Console.WriteLine("Start to read");
                Read_Completed(this, _readEventArgs);

                OnLine = true;
            }
        }

        public void Reconnect()
        {
            _socket.Disconnect(true);
            Init();
            Connect();
        }
        
        public IRajaProvider RajaProvider { get; set; }
        public void SendBytes(Socket socket, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}