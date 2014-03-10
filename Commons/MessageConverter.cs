using System;

namespace Dragon
{
    /// <summary>
    ///     Generic message converter.
    ///     Need factory and CircularBuffer
    /// </summary>
    /// <typeparam name="TAck">Message</typeparam>
    /// <typeparam name="TReq"></typeparam>
    public class MessageConverter<TReq, TAck> : IMessageConverter<TReq, TAck>
    {
        private byte[] _buffer;
        
        private int _offset;
        private int _lastOffset;
        private int _initialOffset;

        private int Stored
        {
            get { return _offset - _initialOffset; }
        }

        private IMessageFactory<TReq, TAck> _factory;

        public IMessageFactory<TReq, TAck> MessageFactory
        {
            set { _factory = value; }
        }
 
        public MessageConverter(byte[] buffer, int offset, int bufferSize, IMessageFactory<TReq, TAck> factory)
        {
            _factory = factory;
            SetBuffer(buffer, offset, bufferSize);
        }

        public void SetBuffer(byte[] buffer, int offset, int bufferSize)
        {
            _buffer = buffer;
            _offset = offset;
            _lastOffset = offset + bufferSize - 1;
            _initialOffset = offset;
        }

        public event Action<TAck, int> ReadCompleted;
        
        public void Convert(byte[] buffer, int offset, int bytesTransferred)
        {
            if (_offset + bytesTransferred > _lastOffset+1)
            {
                throw new OutOfMemoryException(string.Format("Buffer overflow {2}+{0}/{1}",_offset,_lastOffset,bytesTransferred));
            }

            Buffer.BlockCopy(buffer, offset, _buffer, _offset, bytesTransferred);

            //add offset
            _offset += bytesTransferred;

            while (_offset > 2)
            {
                ushort messageLength = BitConverter.ToUInt16(_buffer, 0);

                if (Stored < messageLength) return;

                _errorCode = 0;
                TAck message;
                if (!_factory.Read(_buffer, _initialOffset, messageLength, out message,
                        out _errorCode)) return;

                PullBufferToFront(messageLength);
                if (null != ReadCompleted)
                {
                    ReadCompleted(message, _errorCode);
                }
            } 
        }

        public void GetByte(TReq message, out byte[] messageBytes, out int errorCode)
        { 
            _factory.GetByte(message, out messageBytes, out errorCode);
        }

        int _errorCode;

        //TODO remove something...
        [Obsolete]
        private void PullBufferToFront(ushort messageLength)
        {
            Buffer.BlockCopy(_buffer, _offset, _buffer, _initialOffset, Stored - messageLength);
            _offset -= messageLength;
        }
    }
}