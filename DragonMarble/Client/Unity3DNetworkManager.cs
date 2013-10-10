using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public class NetworkEventArgs : EventArgs
    {
        public IGameMessage Message { get; set; }
        public short MessageLength { get; set; }
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
    }

    public delegate void MessageReceiveEventHandler(object sender, NetworkEventArgs args);

    public delegate void MessageSendEventHandler(object sender, NetworkEventArgs args);

    public class Unity3DNetworkManager
    {
        private const int ResetValue = -1;
        private readonly byte[] _buffer;
        private readonly NetworkEventArgs _receiveEventArgs = new NetworkEventArgs();
        private readonly NetworkEventArgs _sendingEventArgs = new NetworkEventArgs();
        private readonly Queue<IGameMessage> _sendingMessages = new Queue<IGameMessage>();
        private readonly Socket _socket;
        private int _bytesTransferred;
        private Int16 _messageLength;
        private int _messageStartOffset;
        private bool _parsing;

        private bool _started;
        private int _offset;
        
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool OnLine { get; set; }
        public event MessageReceiveEventHandler OnMessageReceived;
        public event MessageSendEventHandler OnMessageSeding;

        public Unity3DNetworkManager(string ipAddress, int port, bool startWhenMade = true)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            IpAddress = ipAddress;
            Port = port;
            _buffer = new byte[1024];
            if (startWhenMade) Start();
        }
        public IEnumerable<IGameMessage> ToMessages(byte[] buffer, int offset, int bytesTransferred)
        {
            var messages = new List<IGameMessage>();

            while (true)
            {
                //start new message
                IGameMessage message = ToMessage(buffer, ref offset, ref bytesTransferred);
                if (null == message) break;
                messages.Add(message);
            }

            return messages;
        }

        private IGameMessage ToMessage(byte[] buffer, ref int offset, ref int bytesTransferred)
        {
            while (true)
            {
                if (_parsing)
                {
                    offset = _messageStartOffset;
                    bytesTransferred += _bytesTransferred;
                    continue;
                }

                _parsing = true;
                _messageStartOffset = offset;
                _bytesTransferred = bytesTransferred;

                //message length is not sufficient
                if (bytesTransferred < 2)
                {
                    return null;
                }
                _messageLength = BitConverter.ToInt16(buffer, offset);

                //message not transferred all
                if (_messageLength > _bytesTransferred)
                {
                    return null;
                }

                //make new game message

                _receiveEventArgs.Buffer = buffer;
                _receiveEventArgs.Offset = offset;
                _receiveEventArgs.MessageLength = _messageLength;

                OnMessageReceived(this, _receiveEventArgs);

                bytesTransferred -= _messageLength;
                offset += _messageLength;

                //reset message offes and etc.
                _messageLength = ResetValue;
                _messageStartOffset = ResetValue;
                _bytesTransferred = ResetValue;
                _parsing = false;

                return _receiveEventArgs.Message;
            }
        }

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


        //TODO this need to be async
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