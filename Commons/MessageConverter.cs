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
        private readonly CircularBuffer _buffer;       

        private readonly IMessageFactory<T> _factory;
        
        public MessageConverter(CircularBuffer buffer, IMessageFactory<T> factory)
        {
            _buffer = buffer;
            _factory = factory;
        }

        // Not to be null.
        public event MessageEventHandler<T> MessageConverted;

        public void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {
            _buffer.CopyFrom(buffer, offset, bytesTransferred);

            while (_buffer.BytesAbleToRead > 2)
            {
                short messageLength = BitConverter.ToInt16(_buffer.PickBytes(2), 0);

                if (_buffer.BytesAbleToRead < messageLength) return;

                T message = _factory.GetMessage(_buffer.GetBytes(messageLength));
                if (null != MessageConverted)
                    MessageConverted(message);
            }
        }
    }
}
