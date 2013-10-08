using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public class Unity3DNetworkManager : NetworkManager
    {
        
        private readonly byte[] _buffer;
        private readonly IMessageProcessor _messageProcessor;
        private readonly Socket _socket;
        private readonly Queue<IGameMessage> _sendingMessages = new Queue<IGameMessage>();
        
        private bool _started;

        public Unity3DNetworkManager(string ipAddress, int port, IMessageParser messageParser,
            IMessageProcessor messageProcessor, bool startWhenMade = true)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            IpAddress = ipAddress;
            Port = port;
            _messageParser = messageParser;
            _messageProcessor = messageProcessor;
            _buffer = new byte[1024];
            if (startWhenMade) Start();
        }

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool OnLine { get; set; }

        public void Start()
        {
            if (_started) return;
            _started = true;
            Connect();
        }

        private void ReceiveCheck()
        {
            if (!OnLine) return;
            //TODO something WTF
            var m = new byte[1024];
            _socket.BeginReceive(m, 0, 1024, SocketFlags.None, CompleteReceive, m);
        }

        private void SendCheck()
        {
            if (_sendingMessages.Count < 1) return;
            byte[] bytes = _sendingMessages.Dequeue().ToByteArray();
            Console.WriteLine(bytes);
            _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, CompleteSend, _socket);
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
                    ((byte[]) ar.AsyncState).CopyTo(_buffer, _offset);
                    _offset += bytesRead;

                    foreach (IGameMessage gameMessage in ToMessages(_buffer, _offset, bytesRead))
                    {
                        _messageProcessor.ProcessMessage(gameMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            ReceiveCheck();
        }

        private void CompleteSend(IAsyncResult ar)
        {
            if (!OnLine) return;
            try
            {
                // Complete sending the data to the remote device.
                _socket.EndSend(ar);
            }
            catch (Exception e)
            {
                //TODO WTF exception 
                Console.WriteLine(e.ToString());
            }
        }


        public void SendMessage(IGameMessage gameMessage)
        {
            _sendingMessages.Enqueue(gameMessage);
            SendCheck();
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
            }
            catch (Exception e)
            {
                //TODO WTF exception 
                Console.WriteLine(e.ToString());
            }
        }

        public void Shutdown()
        {
            OnLine = false;
            if (null != _socket) _socket.Close();
        }

        public byte[] ToByte(IGameMessage message)
        {
            return message.ToByteArray();
        }

        
        }
    }