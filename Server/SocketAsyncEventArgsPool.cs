using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Dragon.Server
{
    public class SocketAsyncEventArgsPool
    {
        private readonly Stack<SocketAsyncEventArgs> _pool;

        // Add a SocketAsyncEventArg instance to the pool 
        // 
        // item's user token release
        //The "item" parameter is the SocketAsyncEventArgs instance 
        // to add to the pool 
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
            lock (_pool)
            {
                item.UserToken = null;
                _pool.Push(item);
            }
        }

        // Removes a SocketAsyncEventArgs instance from the pool 
        // and returns the object removed from the pool 
        public SocketAsyncEventArgs Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        // The number of SocketAsyncEventArgs instances in the pool 
        public int Count
        {
            get { return _pool.Count; }
        }

        // Initializes the object pool to the specified size 
        // 
        // The "capacity" parameter is the maximum number of 
        // SocketAsyncEventArgs objects the pool can hold 
        public SocketAsyncEventArgsPool
            (int capacity, EventHandler<SocketAsyncEventArgs> completed, 
            BufferManager bufferManager = null)
        {
            _pool = new Stack<SocketAsyncEventArgs>(capacity);

            // preallocate pool of SocketAsyncEventArgs objects
            for (int i = 0; i < capacity; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                SocketAsyncEventArgs readWriteEventArg = CreateNew(completed, bufferManager);

                // add SocketAsyncEventArg to the pool
                Push(readWriteEventArg);
            }
        }

        public SocketAsyncEventArgs CreateNew(EventHandler<SocketAsyncEventArgs> completed,
            BufferManager bufferManager = null)
        {
            SocketAsyncEventArgs arg = new SocketAsyncEventArgs(); 
            arg.Completed += completed;

            // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
            if (bufferManager != null) bufferManager.SetBuffer(arg);

            return arg;
        }
    }
}