using System;

namespace Dragon
{
    /// <summary>
    /// Circluar byte buffer
    /// </summary>
    public class CircularBuffer
    {
        private byte[] _container;
        private int _head;
        private int _length;
        private int _tail;

        public CircularBuffer(byte[] container)
        {
            _container = container;
            _length = _container.Length;
        }

        public int BytesAbleToRead
        {
            get { return _head < _tail ? _length - _tail + _head : _head - _tail; }
        }

        public bool IsFull
        {
            get { return (_head + 1) % _length == _tail; }
        }

        public bool IsEmpty
        {
            get { return _head == _tail; }
        }

        public int AbleToWriteLength
        {
            get
            {
                return _head > _tail
                    ? _container.Length - _head
                    : (_tail > 0 ? (_tail - 1) : _length);
            }
        }

        /// <summary>
        ///     Doesn't throw exception
        /// </summary>
        /// <param name="sourceBuffer"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="byteToRead"></param>
        public void CopyFrom(byte[] sourceBuffer, int sourceOffset, int byteToRead)
        {
            while (!IsFull && byteToRead > 0)
            {
                //cache value
                int writeLength = AbleToWriteLength < byteToRead ? AbleToWriteLength : byteToRead;
                Buffer.BlockCopy(sourceBuffer, sourceOffset, _container, _head, writeLength);
                sourceOffset += writeLength;
                byteToRead -= writeLength;
                _head += writeLength;
                _head = _head % _length;
            }
        }

        public byte[] GetBytes(int bytesLength)
        {
            byte[] lenghtBytes = new byte[bytesLength];
            for (int i = 0; i < bytesLength; i++)
            {
                lenghtBytes[i] = _container[_tail];
                _tail = (_tail + 1) % _length;
            }
            return lenghtBytes;
        }

        public byte[] PickBytes(int bytesLength)
        {
            int tail = _tail;
            byte[] lenghtBytes = new byte[bytesLength];
            for (int i = 0; i < bytesLength; i++)
            {
                lenghtBytes[i] = _container[tail];
                tail = (tail + 1) % _length;
            }
            return lenghtBytes;
        }
    }
}
