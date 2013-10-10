using System;
using System.Collections.Generic;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public class NetworkEventArgs : EventArgs
    {
        public Boolean Result { get; set; }
    }

    public class SimpleNetworkEventArgs : NetworkEventArgs
    {
        public IGameMessage GameMessage { get; set; }
    }

    public class QueueNetworkEventArgs : NetworkEventArgs
    {
        private readonly Queue<IGameMessage> _messages = new Queue<IGameMessage>();

        private const int ResetValue = -1;
        private int _bytesTransferred;
        private Int16 _messageLength;
        private int _messageStartOffset;
        private bool _parsing;

        public QueueNetworkEventArgs()
        {
            Buffer = new byte[1024];
        }

        public IGameMessage Message
        {   
            get
            {
                if (IsEmpty()) return null;
                return _messages.Dequeue();
            }
            set
            {
                _messages.Enqueue(value);
            }
        }

        public short MessageLength { get; set; }
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }

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

                //_receiveEventArgs.Buffer = buffer;
                //_receiveEventArgs.Offset = offset;
                //_receiveEventArgs.MessageLength = _messageLength;

                //OnAfterMessageReceive(this, _receiveEventArgs);

                bytesTransferred -= _messageLength;
                offset += _messageLength;

                //reset message offes and etc.
                _messageLength = ResetValue;
                _messageStartOffset = ResetValue;
                _bytesTransferred = ResetValue;
                _parsing = false;

                return Message;
            }
        }
        
        public void ToMessage()
        {
            
        }

        public bool IsEmpty()
        {
            return _messages.Count < 1;
        }
    }
}