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

        private int _state;

        public SocketState State
        {
            get { return (SocketState) _state; }
            set { _state = (int) value; }
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

        protected abstract void ReadEventCompleted(object sender,
            SocketAsyncEventArgs readEventArgs);

        /// <summary>
        ///     Read repeat
        /// </summary>
        protected void ReadRepeat()
        {
            if (Socket.ReceiveAsync(_readEventArgs)) return;
            ReadEventCompleted(Socket, _readEventArgs);
        }

        private SocketAsyncEventArgs _writeEventArgs;

        protected abstract void WriteEventCompleted(object socket,
            SocketAsyncEventArgs readEventArgs);

        public event EventHandler<SocketAsyncEventArgs> Disconnected;

        /// <summary>
        ///     For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect(SocketAsyncEventArgs e = null)
        {
            var exchange =
                (SocketState)
                    Interlocked.Exchange(ref _state,
                        (byte) SocketState.Disconnected);
            //run once
            if (exchange <= SocketState.Disconnected) return;

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Disconnect(false);
            }
            catch (SocketException ex)
            {
                //ignore already disconnected 

                //ignore something else
            }
            catch (ObjectDisposedException ex)
            {
                //ignore already disposed
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
                _readEventArgs.Dispose();
            }
            catch (Exception ex)
            {
                //ignore some error
            }
        }

        public virtual void Dispose()
        {
            if (State <= SocketState.Disposed) return;

            Disconnect(); 

            State = SocketState.Disposed;
        }

        public virtual void Activate()
        {
            _readEventArgs = new SocketAsyncEventArgs();

            _readEventArgs.SetBuffer(_buffer, _offset, _bufferSize);

            _readEventArgs.Completed += IOCompleted;
            _readEventArgs.Completed += ReadEventCompleted;
            _readEventArgs.DisconnectReuseSocket = true;

            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += IOCompleted;
            _writeEventArgs.Completed += WriteEventCompleted;
            _writeEventArgs.DisconnectReuseSocket = true;

            State = SocketState.Active;

            ReadRepeat();
        }

        protected void SendAsync(byte[] byteArray)
        {
            try
            {
                _writeEventArgs.SetBuffer(byteArray, 0, byteArray.Length);
                if (Socket.SendAsync(_writeEventArgs)) return;
                WriteEventCompleted(Socket, _writeEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                if (State == SocketState.Active)
                {
                    throw new InvalidOperationException(
                        "Socket State is Active. But socket disposed.", e);
                }
            }
        }
        
        // ReSharper disable once InconsistentNaming
        private void IOCompleted(object sender, SocketAsyncEventArgs args)
        {
            switch (args.SocketError)
            {
                case SocketError.Success:
                    break;
                case SocketError.IsConnected:
                case SocketError.NetworkReset:
                case SocketError.ConnectionAborted:
                case SocketError.ConnectionReset:
                case SocketError.Disconnecting:
                case SocketError.NotConnected:
                case SocketError.Shutdown:
                case SocketError.TimedOut:
                    Disconnect(args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(String.Format("socket error : {0}, lastOperation: {1}", args.SocketError, args.LastOperation));
            }
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