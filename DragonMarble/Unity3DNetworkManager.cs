using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DragonMarble
{
    public class Unity3DNetworkManager : IDisposable
    {
        private readonly IMessageParser _messageParser;
        private readonly IEnumerable<GameMessage> _receiveMessages = new Queue<GameMessage>();
        private readonly Queue<GameMessage> _sendMessages = new Queue<GameMessage>();
        private readonly Socket _socket;
        private Thread _receiveCheck;
        private Thread _sendCheck;

        public Unity3DNetworkManager(string ipAddress, int port, IMessageParser messageParser)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            IpAddress = ipAddress;
            Port = port;
            _messageParser = messageParser;
            Start();
        }

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool OnLine { get; set; }

        public void Dispose()
        {
            Shutdown();
        }

        private void Start()
        {
            Connect();
        }

        private void ReceiveCheck()
        {
            while (OnLine)
            {
                var m = new byte[1024];
                _socket.BeginReceive(m, 0, 1024, SocketFlags.None, CompleteReceive, m);

                Thread.Sleep(300);
            }
        }

        private void SendCheck()
        {
            while (OnLine)
            {
                if (_sendMessages.Count < 1) continue;

                byte[] bytes = _sendMessages.Dequeue().Contents;
                _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, CompleteSend, _socket);

                Thread.Sleep(300);
            }
        }

        private void CompleteReceive(IAsyncResult ar)
        {
            if (!OnLine) return;
            try
            {
                // Read data from the remote device.
                int bytesRead = _socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    String a = Encoding.ASCII.GetString((byte[]) ar.AsyncState, 0, bytesRead);

                    _messageParser.SetMessage(a);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void CompleteSend(IAsyncResult ar)
        {
            if (!OnLine) return;
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = _socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendMessage(GameMessage message)
        {
            _sendMessages.Enqueue(message);
        }

        public void Connect()
        {
            _socket.BeginConnect(IpAddress, Port, ConnectCallback, _socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Complete the connection.
                _socket.EndConnect(ar);
                OnLine = true;
                _receiveCheck = new Thread(ReceiveCheck);
                _sendCheck = new Thread(SendCheck);
                _receiveCheck.Start();
                _sendCheck.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Shutdown()
        {
            OnLine = false;
            if (null != _sendCheck && _sendCheck.IsAlive) _sendCheck.Abort();
            if (null != _receiveCheck && _receiveCheck.IsAlive) _receiveCheck.Abort();
            if (null != _socket) _socket.Close();
        }
    }
}