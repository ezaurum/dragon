using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Dragon
{
    public abstract class AbstractDragonSocket<T> : IDragonSocketMinimal<T>
    {
        private const int BufferSize = 16 * 1024;

        protected AbstractDragonSocket()
        {
            _readEventArgs = new SocketAsyncEventArgs();

            //TODO is this need pool?
            _readEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            _readEventArgs.Completed += OnReadEventArgsOnCompleted;
            _readEventArgs.DisconnectReuseSocket = true;

            WriteEventArgs = new SocketAsyncEventArgs();
            WriteEventArgs.Completed += OnWriteEventArgsOnCompleted;
            WriteEventArgs.DisconnectReuseSocket = true;

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
        
        private readonly SocketAsyncEventArgs _readEventArgs;
        protected abstract void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs readEventArgs);

        /// <summary>
        ///     Read repeat
        /// </summary>
        protected void ReadRepeat()
        {
            if (Socket.ReceiveAsync(_readEventArgs)) return;
            OnReadEventArgsOnCompleted(Socket, _readEventArgs);
        }

        protected readonly SocketAsyncEventArgs WriteEventArgs;
        private readonly object _lock = new object();
        
        public abstract void Send(T message);
        protected abstract void OnWriteEventArgsOnCompleted(object socket, SocketAsyncEventArgs readEventArgs);

        public event EventHandler<SocketAsyncEventArgs> OnDisconnected;

        public void Disconnect()
        {
            Disconnect(null);
        }

        /// <summary>
        ///     For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect(SocketAsyncEventArgs e)
        {
            SocketState exchange =
                (SocketState)
                    Interlocked.Exchange(ref _state,
                        (byte) SocketState.Disconnected);

            //run once
            if (exchange <= SocketState.Disconnected) return;

            try
            { 
                Socket.Shutdown(SocketShutdown.Both); 
                Socket.Disconnect(true);
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

            if (null == OnDisconnected) return;

            OnDisconnected(this, e);
        }

        public virtual void Dispose()
        {
            if (State <= SocketState.Disposed) return;

            Disconnect();

            try
            {
                WriteEventArgs.Dispose();
                _readEventArgs.Dispose();
            }
            catch
            {
                //ignore some error
            }
            
            State = SocketState.Disposed;
        }

        public virtual void Activate()
        {
            State = SocketState.Active;
            ReadRepeat();
        }


        /// <summary>
        ///     debug event args for log or something
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> DebugEvent
        {
            add { _readEventArgs.Completed += value; }
            remove { _readEventArgs.Completed -= value; }
        }
    }
}