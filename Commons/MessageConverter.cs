using System;

namespace Dragon
{
    /// <summary>
    /// Generic message converter.
    /// Need factory and CircularBuffer
    /// </summary>
    /// <typeparam name="T">Message</typeparam>
    public class MessageConverter<T> : IMessageConverter<T> where T:IMessage
    {
        private readonly byte[] _buffer;

        private readonly IMessageFactory<T> _factory;
        

        public MessageConverter(IMessageFactory<T> factory)
        {
            _buffer = new byte[2048];
            _factory = factory;
        }

        // Not to be null.
        public event MessageEventHandler<T> MessageConverted;

        public void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {
            Buffer.BlockCopy(buffer, offset, _buffer, _offset, bytesTransferred);

            //add offset
            _offset += bytesTransferred;

            while (_offset > 2)
            {
                short messageLength = BitConverter.ToInt16(_buffer, 0);

                if (_offset < messageLength) return;

                T message = _factory.GetMessage(_buffer, 0, messageLength);

                //after converted. pull buffer to front
                Buffer.BlockCopy(_buffer, messageLength, _buffer, 0, _offset - messageLength);
                _offset -= messageLength;

                if (null != MessageConverted)
                    MessageConverted(message);
            }
        }

        private int _offset;
    }
}
