using System;
using System.Net;
using System.Net.Sockets;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public delegate void SocketAsyncEventHandler(object sender, SocketAsyncEventArgs e);
    public class Unity3DNetworkManager
    {
        private readonly Socket _socket;
        
        private bool _started;
        private SocketAsyncEventArgs _connectEventArgs;
        private SocketAsyncEventArgs _readEventArgs; 
        private SocketAsyncEventArgs _writeEventArgs;
        private EndPoint _ipEndpoint;

        public bool OnLine { get; set; }
        
        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageSend;
        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageReceive;

        public Unity3DNetworkManager(string ipAddress, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            _ipEndpoint = new IPEndPoint( IPAddress.Parse(ipAddress), port);
        }
        
        public void SendMessage(IGameMessage gameMessage)
        {
            byte[] byteArray = gameMessage.ToByteArray();
            byteArray.CopyTo(_writeEventArgs.Buffer, _writeEventArgs.Offset);
            
            if (!_socket.SendAsync(_writeEventArgs))
            {
                CompleteSend(_writeEventArgs);
            }
        }

        private void CompleteSend(SocketAsyncEventArgs writeEventArgs)
        {
            OnAfterMessageSend(this, writeEventArgs);
        }

        public void Start()
        {
            if (_started) return;

            Init();

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
            _readEventArgs.Completed += OnAfterMessageReceive;
            _readEventArgs.Completed += Read_Completed;
            _readEventArgs.SetBuffer(new byte[1024], 0, 1024);
            

            if (null == OnAfterMessageSend)
            {
                throw new InvalidOperationException("OnAfterMessageSend is not setted.");
            }
            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += OnAfterMessageSend;
            _writeEventArgs.SetBuffer(new byte[1024], 0, 1024);

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
            if (!((QueueAsyncClientUserToken) e.UserToken).Socket.ReceiveAsync(e))
            {
                OnAfterMessageReceive(this, e);
                Read_Completed(this, e);
            }
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            OnLine = true;
            
            Socket socket = e.AcceptSocket;
            QueueAsyncClientUserToken readClientUserTokent 
                = new QueueAsyncClientUserToken {Socket = socket};
            SimpleAsyncClientUserToken writeClientUserToken 
                = new SimpleAsyncClientUserToken {Socket = socket};
            _readEventArgs.UserToken = readClientUserTokent;
            _writeEventArgs.UserToken = writeClientUserToken;

            socket.ReceiveAsync(_readEventArgs);
            socket.SendAsync(_writeEventArgs);
        }
    }
}