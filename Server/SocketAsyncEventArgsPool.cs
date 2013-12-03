using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Dragon.Server
{
    // This class creates a single large buffer which can be divided up 
    // and assigned to SocketAsyncEventArgs objects for use with each 
    // socket I/O operation.  
    // This enables bufffers to be easily reused and guards against 
    // fragmenting heap memory.
    // 
    // The operations exposed on the BufferManager class are not thread safe.
    public class BufferManager
    {
        private readonly int _numBytes;                 // the total number of bytes controlled by the buffer pool
        private byte[] _buffer;                // the underlying byte array maintained by the Buffer Manager
        private readonly Stack<int> _freeIndexPool;     // 
        private int _currentIndex;
        private readonly int _bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            _numBytes = totalBytes;
            _currentIndex = 0;
            _bufferSize = bufferSize;
            _freeIndexPool = new Stack<int>();

            // Allocates buffer space used by the buffer pool
            // create one big large buffer and divide that 
            // out to each SocketAsyncEventArg object
            _buffer = new byte[_numBytes];
        }

        // Assigns a buffer from the buffer pool to the 
        // specified SocketAsyncEventArgs object
        //
        // <returns>true if the buffer was successfully set, else false</returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {

            if (_freeIndexPool.Count > 0)
            {
                args.SetBuffer(_buffer, _freeIndexPool.Pop(), _bufferSize);
            }
            else
            {
                if ((_numBytes - _bufferSize) < _currentIndex)
                {
                    return false;
                }
                args.SetBuffer(_buffer, _currentIndex, _bufferSize);
                _currentIndex += _bufferSize;
            }
            return true;
        }

        // Removes the buffer from a SocketAsyncEventArg object.  
        // This frees the buffer back to the buffer pool
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            _freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }    

    public class SocketAsyncEventArgsPool
    {
        private Stack<SocketAsyncEventArgs> _pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            Prepare(capacity);
        }

        public SocketAsyncEventArgsPool()
        {
        }

        public BufferManager BufferManager { get; set; }

        public int Count
        {
            get { return _pool.Count; }
        }

        public int Capacity { get; set; }

        // Add a SocketAsyncEventArg instance to the pool 
        // 
        // item's user token release
        //The "item" parameter is the SocketAsyncEventArgs instance 
        // to add to the pool 
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }
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

        public event EventHandler<SocketAsyncEventArgs> Completed;
        
        public void Prepare(int capacity)
        {
            _pool = new Stack<SocketAsyncEventArgs>(capacity);

            // preallocate pool of SocketAsyncEventArgs objects
            for (int i = 0; i < capacity; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                SocketAsyncEventArgs readWriteEventArg = CreateNew();

                // add SocketAsyncEventArg to the pool
                Push(readWriteEventArg);
            }
        }

        public SocketAsyncEventArgs CreateNew()
        {
            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();

            arg.Completed += Completed;

            // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
            if (BufferManager != null) BufferManager.SetBuffer(arg);

            return arg;
        }
    }
}