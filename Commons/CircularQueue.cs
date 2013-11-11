using System;

namespace Dragon.Interfaces
{
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
            _head = 0;
            _tail = 0;
            Extend(i);
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