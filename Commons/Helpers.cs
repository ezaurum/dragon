using System;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    /// Circluar byte buffer
    /// </summary>
    public class CircluarBuffer
    {
        private byte[] _container;
        private int _head;
        private int _length;
        private int _tail;

        public CircluarBuffer(byte[] container)
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

    public class CircularQueue<T>
    {
        private T[] _container;
        private int _head;
        private int _tail;
        private int _length;

        public CircularQueue() : this(5)
        {
          
        }

        public CircularQueue(int i)
        {
            Extend(i);
        }

        public CircularQueue(T[] container)
        {
            _container = container;
        }

        public void Extend(int size)
        {
            _length = size;
            _container = new T[_length];
        }

        public int Count
        {
            get { return _head < _tail ? _length - _tail + _head : _head - _tail; }
        }

        public bool Enqueue(T t)
        {
            if (null == t) 
                throw new ArgumentNullException(String.Format("Parameter cannot be null. {0}",t.GetType()));
            if ( IsFull)
                throw new ArgumentOutOfRangeException(String.Format("Queue is full. {0}",t.GetType()));
            _container[_head] = t;
            _head = (_head + 1) % _length;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if ( IsEmpty )
                throw new InvalidOperationException("No data.");

            T result = _container[_tail];
            _tail = (_tail + 1) % _length;
            return result;
        }

        public bool IsFull
        {
            get
            {
                if (((_head + 1) % _length) == _tail)
                    return true;

                return false;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (_head == _tail)
                    return true;

                return false;
            }
        }
    }
}