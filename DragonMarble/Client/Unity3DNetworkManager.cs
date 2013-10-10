using System;
using System.Net.Sockets;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public delegate void MessageEventHandler(object sender, NetworkEventArgs args);

    public class Unity3DNetworkManager
    {
        
        private readonly NetworkEventArgs _receiveEventArgs = new NetworkEventArgs();
        private readonly NetworkEventArgs _sendingEventArgs = new NetworkEventArgs();
        private readonly Socket _socket;
        
        private bool _started;

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool OnLine { get; set; }
        
        public event MessageEventHandler OnBeforeMessageSend;
        public event MessageEventHandler OnAfterMessageSend;
        public event MessageEventHandler OnBeforeMessageReceive;
        public event MessageEventHandler OnAfterMessageReceive;
        public event MessageEventHandler OnMessageToBytes;
        public event MessageEventHandler OnBytesToMessage;

        public Unity3DNetworkManager(string ipAddress, int port, bool startWhenMade = true)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            IpAddress = ipAddress;
            Port = port;
            if (startWhenMade) Start();
        }
        
        public void SendMessage(IGameMessage gameMessage)
        {
            _sendingEventArgs.Message = gameMessage;
            OnBeforeMessageSend(_socket, _sendingEventArgs);
            OnMessageToBytes(_socket, _sendingEventArgs);

            _socket.BeginSend(_sendingEventArgs.Buffer, 
                _sendingEventArgs.Offset, _sendingEventArgs.MessageLength, 
                SocketFlags.None, CompleteSend, _socket);
        }

        private void CompleteSend(IAsyncResult ar)
        {
            if (!OnLine) return;
            //Complete sending the data to the remote device.
            _socket.EndSend(ar);
            OnAfterMessageSend(this, _sendingEventArgs);
        }

        public void Start()
        {
            if (_started) return;
            
            if (null == OnMessageToBytes || OnMessageToBytes.GetInvocationList().Length < 1)
            {
                throw new InvalidOperationException("OnMessageToBytes is not set.");
            }
            if (null == OnBytesToMessage || OnBytesToMessage.GetInvocationList().Length < 1)
            {
                throw new InvalidOperationException("OnBytesToMessage is not set.");
            }

            _receiveEventArgs.OnEnqueue += OnAfterMessageReceive;
            _sendingEventArgs.OnDequeue += OnAfterMessageSend;

            _started = true;
            Connect();
        }

        private void ReceiveCheck()
        {
            if (!OnLine) return;
            OnBeforeMessageReceive(this, _receiveEventArgs);
            _socket.BeginReceive(_receiveEventArgs.Buffer, _receiveEventArgs.Offset, 1024, 
                SocketFlags.None, CompleteReceive, _receiveEventArgs.Buffer);
        }

        private void CompleteReceive(IAsyncResult ar)
        {
            if (!OnLine) return;
            
            // Read data from the remote device.
            int bytesRead = _socket.EndReceive(ar);

            if (bytesRead > 0)
            {
                OnBytesToMessage(this, _receiveEventArgs);
            }
            ReceiveCheck();
        }
        
        public void Connect()
        {
            _socket.BeginConnect(IpAddress, Port, ConnectCallback, _socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            //Complete the connection.
            _socket.EndConnect(ar);
            OnLine = true;
            
            //after connected. receive message
            ReceiveCheck();
        }

        public void Shutdown()
        {
            OnLine = false;
            if (null != _socket) _socket.Close();
        }
    }
}