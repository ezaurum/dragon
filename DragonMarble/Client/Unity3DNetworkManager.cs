using System;
using System.Net.Sockets;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public delegate void MessageEventHandler(object sender, NetworkEventArgs args);

    public class Unity3DNetworkManager
    {

        private readonly QueueNetworkEventArgs _receiveEventArgs = new QueueNetworkEventArgs();
        private readonly SimpleNetworkEventArgs _sendEventArgs = new SimpleNetworkEventArgs();
        private readonly Socket _socket;
        
        private bool _started;

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool OnLine { get; set; }
        
        public event MessageEventHandler OnAfterMessageSend;
        public event MessageEventHandler OnAfterMessageReceive;

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
            byte[] byteArray = gameMessage.ToByteArray();
            _sendEventArgs.GameMessage = gameMessage;
            _socket.BeginSend(byteArray, 0, byteArray.Length,
                SocketFlags.None, CompleteSend, _socket);
        }

        private void CompleteSend(IAsyncResult ar)
        {
            if (!OnLine) return;
            //Complete sending the data to the remote device.
            _socket.EndSend(ar);
            _sendEventArgs.Result = ar.IsCompleted;
            OnAfterMessageSend(this, _sendEventArgs);
        }

        public void Start()
        {
            if (_started) return;

            if (null == OnAfterMessageReceive)
            {
                throw new InvalidOperationException("OnAfterMessageReceive is not setted.");
            }

            if (null == OnAfterMessageSend)
            {
                throw new InvalidOperationException("OnAfterMessageSend is not setted.");
            }

            _started = true;
            Connect();
        }

        private void ReceiveCheck()
        {
            if (!OnLine) return;
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
                OnAfterMessageReceive(this, _receiveEventArgs);
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