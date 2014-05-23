using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Dragon
{
    /// <summary>
    /// Byte stream socket wrapper.
    /// send byte stream and receive byte stream
    /// </summary>
    public abstract class ByteStreamSocketWrapper : ISocketWrapper
    {
        private readonly byte[] _buffer;
        private readonly int _offset;
        private readonly int _bufferSize;

        protected ByteStreamSocketWrapper(byte[] buffer, int offset, int bufferSize)
        {
            _buffer = buffer;
            _offset = offset;
            _bufferSize = bufferSize; 

            State = SocketState.Initialized;
        }

        private SocketState _state;

        public SocketState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint) Socket.RemoteEndPoint; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint) Socket.LocalEndPoint; }
        }

        protected Socket Socket { set; get; }

        private SocketAsyncEventArgs _readEventArgs;

        /// <summary>
        ///     Read repeat
        /// </summary>
        private void ReadRepeat()
        {
            try
            {
                if (Socket.ReceiveAsync(_readEventArgs)) return;
                ReadEventCompleted(Socket, _readEventArgs);
            }
            catch (Exception e)
            {
                //ignore disposed
                Disconnect();
            }
        }

        private SocketAsyncEventArgs _writeEventArgs;
        private readonly object _stateLock = new object();

        protected abstract void WriteEventCompleted(object socket,
            SocketAsyncEventArgs readEventArgs);

        public event EventHandler<SocketAsyncEventArgs> Disconnected;

        protected bool OffState(SocketState state)
        {
            lock (_stateLock)
            {
                if ((_state & state) == 0)
                    return false;
                _state ^= state;
                return true;
            } 
        }

        protected bool OnState(SocketState state)
        {
            lock (_stateLock)
            {
                if ((_state & state) != 0)
                    return false;
                _state |= state;
                return true;
            }
        }

        protected bool IsState(SocketState state)
        {
            lock (_stateLock)
            {
                return (_state & state) != 0;
            }
        }

        /// <summary>
        ///     For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect(SocketAsyncEventArgs e = null)
        {
            if (!OffState(SocketState.Connected)) return;
            try
            {
                Socket.Shutdown(SocketShutdown.Send);
                Socket.Disconnect(false);
            }
            catch
            {
                //ignore
            }

            DisposeInner();

            if (null == Disconnected) return;
            
            Disconnected(this, e);
        }

        private void DisposeInner()
        {
            try
            {
                _writeEventArgs.Dispose(); 
            }
            catch
            {
                //ignore some error
            }
            try
            {
                _readEventArgs.Dispose();
            }
            catch
            {
                //ignore some error
            }
        }

        public virtual void Dispose()
        {
            Disconnect();

            lock (_stateLock)
            {
                _state = SocketState.Disposed;
            }
        }

        public virtual void Activate()
        {
            _readEventArgs = new SocketAsyncEventArgs();

            _readEventArgs.SetBuffer(_buffer, _offset, _bufferSize);

            _readEventArgs.Completed += IOCompleted;
            _readEventArgs.Completed += ReadEventCompleted;

            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += IOCompleted;
            _writeEventArgs.Completed += WriteEventCompleted;

            State = SocketState.Active;

            ReadRepeat();
        }

        protected void SendAsync(byte[] byteArray)
        {   
            try
            {
                _writeEventArgs.SetBuffer(byteArray, 0, byteArray.Length);
                if (Socket.SendAsync(_writeEventArgs)) return;
            }
            catch (Exception e)
            {
                Disconnect(_writeEventArgs);
            }
            WriteEventCompleted(Socket, _writeEventArgs);
        }
        
        // ReSharper disable once InconsistentNaming
        private void IOCompleted(object sender, SocketAsyncEventArgs args)
        {
            Console.WriteLine("tr : {1}/{0}", args.BytesTransferred, args.SocketError);

            if (0 !=args.BytesTransferred && args.SocketError == SocketError.Success)
                return;

            Disconnect(args);
        }

        public event EventHandler<SocketAsyncEventArgs> OnReadCompleted;
        
        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="args"></param>
        private void ReadEventCompleted(object socket,
            SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success) return;

            if (args.BytesTransferred < 1) return;

            try
            {
                OnReadCompleted(socket, args);

                args.SetBuffer(args.Offset, args.Count);
            }
            catch (Exception ex)
            {
                //ignore?
                Disconnect();
                return;
            }

            ReadRepeat();
        }


#if DEBUG

        /// <summary>
        ///     debug event args for log or something
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> DebugReadEvent
        {
            add { _readEventArgs.Completed += value; }
            remove { _readEventArgs.Completed -= value; }
        }

        /// <summary>
        ///     debug event args for log or something
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> DebugWriteEvent
        {
            add { _writeEventArgs.Completed += value; }
            remove { _writeEventArgs.Completed -= value; }
        }

        /// <summary>
        ///     debug event args for log or something
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> DebugEvent;
#endif
    }
}