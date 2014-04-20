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

        protected abstract void WriteEventCompleted(object socket,
            SocketAsyncEventArgs readEventArgs);

        public event EventHandler<SocketAsyncEventArgs> Disconnected;

        /// <summary>
        ///     For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect(SocketAsyncEventArgs e = null)
        { 
            Console.WriteLine("public void Disconnect(SocketAsyncEventArgs e = null)");

            var exchange =
                (SocketState)
                    Interlocked.Exchange(ref _state,
                        (byte) SocketState.Disconnected); 
            //run once
            if (exchange <= SocketState.Disconnected) return;
            
            Socket.Shutdown(SocketShutdown.Send);
            Socket.Disconnect(false);

            DisposeInner();

            if (null == Disconnected) return;

            Console.WriteLine("call dis");
            Disconnected(this, e);
        }

        private void DisposeInner()
        {
            try
            {
                _writeEventArgs.Dispose(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("write dispose");
                //ignore some error
            }
            try
            {
                _readEventArgs.Dispose();
            }
            catch (Exception)
            {
                Console.WriteLine("read dispose");
                //ignore some error
            }
        }

        public virtual void Dispose()
        {
            Disconnect();

            Interlocked.Exchange(ref _state, (int) SocketState.Disposed);
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
            _writeEventArgs.SetBuffer(byteArray, 0, byteArray.Length);
            try
            {
                if (Socket.SendAsync(_writeEventArgs)) return;
            }
            catch (Exception e)
            {
                Console.WriteLine("excpetion whil send");
                Disconnect(_writeEventArgs);
            }
            WriteEventCompleted(Socket, _writeEventArgs);
        }
        
        // ReSharper disable once InconsistentNaming
        private void IOCompleted(object sender, SocketAsyncEventArgs args)
        {
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

            OnReadCompleted(socket, args);

            try
            {
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