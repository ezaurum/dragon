using System;

namespace DragonMarble
{
    public class CircularQueue<T>
    {
        private T[] _container;
        private int _head;
        private int _tail;
        private int _length;

        public CircularQueue()
        {
            _head = 0;
            _tail = 0;
            Extend(5);
        }

        public void Extend(int size)
        {
            _length = size;
            _container = new T[_length];
        }

        public int Count
        {
            get { return _head > _tail  ? _head - _tail : _tail - _head; }
        }

        public bool Enqueue(T t)
        {
            if ( null == t) 
                throw new InvalidOperationException("Parameter cannot be null.");
            
            _head = (_head + 1) % _length;
            _container[_head] = t;
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

            _tail = (_tail+1) % _length;
            return _container[_tail];
        }

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