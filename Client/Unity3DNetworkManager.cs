using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Dragon.Message;

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

        public bool OnLine { get; set; }
        
        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageSend;
        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageReceive;

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
            
            _writeEventArgs.SetBuffer(byteArray, 0, byteArray.Length);
            try

            {
                if (!_socket.SendAsync(_writeEventArgs))
                {
                    Send_Completed(_writeEventArgs);
                }
            }
            catch (InvalidOperationException e)
            {
                if (typeof (ObjectDisposedException) == e.GetType())
                {
                    Reconnect();
                }
                Thread.Sleep(1);
            }
            catch (SocketException e)
            {
                Reconnect();
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
            Console.WriteLine("READ_COMPLETED");
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                Console.WriteLine("Has Data {0}",e.BytesTransferred);
                OnAfterMessageReceive(sender, e);
                Console.WriteLine("Recursive READ");
                
            } else if (e.SocketError != SocketError.Success)
            {
                OnLine = false;
                return;
            }

            if (!_socket.ReceiveAsync(e))
            {
                Read_Completed(sender, e);
            }
            
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            OnLine = true;

            if (e.SocketError == SocketError.Success)
            {
                _readEventArgs.UserToken = RajaProvider.NewInstance();
                _writeEventArgs.UserToken = ((ClientRajaProvider)RajaProvider).NewWriteAsyncUserToken();
                
                Console.WriteLine("Start to read");
                Read_Completed(this, _readEventArgs);
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