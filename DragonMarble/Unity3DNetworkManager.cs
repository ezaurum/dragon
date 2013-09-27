using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DragonMarble
{
    public class Unity3DNetworkManager
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

        public static void Log(string message, Object context = null)
        {
#if UNITY_EDITOR
    Debug.Log(message, context);
#else
            Console.WriteLine(message, context);
#endif
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
            try
            {
                if (!OnLine) return;
                var message = (byte[]) ar.AsyncState;
                // Read data from the remote device.
                int bytesRead = _socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    String a = Encoding.ASCII.GetString(message, 0, bytesRead);

                    _messageParser.SetMessage(a);
                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        private void CompleteSend(IAsyncResult ar)
        {
            try
            {
                if (!OnLine) return;
                // Complete sending the data to the remote device.
                int bytesSent = _socket.EndSend(ar);
                Log("Sent {0} bytes to server.", bytesSent);
            }
            catch (Exception e)
            {
                Log(e.ToString());
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
                Log("Connected.");
                OnLine = true;
                _receiveCheck = new Thread(ReceiveCheck);
                _receiveCheck.Start();
                _sendCheck = new Thread(SendCheck);
                _sendCheck.Start();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        public void Shutdown()
        {
            Log("shutdown");
            _receiveCheck.Abort();
            _sendCheck.Abort();
            Log("abort");
            OnLine = false;
            _socket.Close();
        }
    }

    public interface IMessageParser
    {
        void SetMessage(string message);
    }
}