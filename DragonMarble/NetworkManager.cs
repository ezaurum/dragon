using System;
using System.Collections.Generic;
using Dragon.Interfaces;

namespace Dragon
{
    public class NetworkManager
    {
        protected IMessageParser _messageParser;

        private const int ResetValue = -1;
        private int _bytesTransferred;
        private Int16 _messageLength;
        private int _messageStartOffset;
        protected int _offset;
        private bool _parsing;

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
                IGameMessage result = _messageParser.MakeNewMessage(buffer, offset, _messageLength);

                bytesTransferred -= _messageLength;
                offset += _messageLength;

                //reset message offes and etc.
                _messageLength = ResetValue;
                _messageStartOffset = ResetValue;
                _bytesTransferred = ResetValue;
                _parsing = false;

                return result;
            }
        }
    }
}