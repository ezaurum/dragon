using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Dragon.Message;
using Dragon.Session;

namespace Dragon.Client
{
    public class Unity3DNetworkManager : INetworkManager
    {
        private readonly Socket _socket;
        
        private bool _started;
        private SocketAsyncEventArgs _connectEventArgs;
        private SocketAsyncEventArgs _readEventArgs; 
        private SocketAsyncEventArgs _writeEventArgs;
        private readonly EndPoint _ipEndpoint;
        private readonly object _lock = new object();

        public bool OnLine { get; set; }

        //session key 
        public GameSession GameSession { get; set; }
        
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
        
        public void SendMessage(IGameMessage gameMessage)
        {
            byte[] byteArray = gameMessage.ToByteArray();
            
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

        public static void DisplayTypeAndAddress()
        {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            Console.WriteLine("Interface information for {0}.{1}     ",
                    computerProperties.HostName, computerProperties.DomainName);
            foreach (NetworkInterface adapter in nics)
            {
                PhysicalAddress physicalAddress = adapter.GetPhysicalAddress();
                IPInterfaceProperties properties = adapter.GetIPProperties();
                Console.WriteLine(adapter.Description);
                Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                Console.WriteLine("  Physical Address ........................ : {0}",
                           physicalAddress.ToString());
                Console.WriteLine("  Is receive only.......................... : {0}", adapter.IsReceiveOnly);
                Console.WriteLine("  Multicast................................ : {0}", adapter.SupportsMulticast);
                Console.WriteLine();
            }
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
}