using System;
using System.Net;
using System.Net.Sockets;

namespace Dragon
{
    public abstract class AbstractDragonSocket
    {
        public AbstractDragonSocket()
        {
            ReadEventArgs = new SocketAsyncEventArgs();

            //TODO is this need pool?
            ReadEventArgs.SetBuffer(new byte[2048], 0, 2048);
            ReadEventArgs.Completed += OnReadEventArgsOnCompleted;

            WriteEventArgs = new SocketAsyncEventArgs();
            WriteEventArgs.Completed += OnWriteEventArgsOnCompleted;

            State = SocketState.Initialized;
        }

        public SocketState State { get; set; }

        public IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint) Socket.RemoteEndPoint; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint) Socket.LocalEndPoint; }
        }

        /// <summary>
        ///     debug event args for log or something
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> DebugEvent
        {
            add { ReadEventArgs.Completed += value; }
            remove { ReadEventArgs.Completed -= value; }
        }

        protected Socket Socket { set; get; }
        protected SocketAsyncEventArgs WriteEventArgs;
        protected SocketAsyncEventArgs ReadEventArgs;

        protected event EventHandler<SocketAsyncEventArgs> OnAbstractDisconnected;


        /// <summary>
        ///     For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect(SocketAsyncEventArgs e = null)
        {
            //run once
            if (State <= SocketState.Disconnected) return;

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Disconnect(true);
            }
            catch (ObjectDisposedException)
            {
                //ignore already disposed
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException("Exception in disconnect. " + ex.ErrorCode, ex);
            }

            State = SocketState.Disconnected;

            if (null == OnAbstractDisconnected) return;

            OnAbstractDisconnected(this, e);
        }

        /// <summary>
        ///     Read repeat
        /// </summary>
        protected void ReadRepeat()
        {
            if (Socket.ReceiveAsync(ReadEventArgs)) return;
            OnReadEventArgsOnCompleted(Socket, ReadEventArgs);
        }

        protected abstract void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs readEventArgs);
        protected abstract void OnWriteEventArgsOnCompleted(object socket, SocketAsyncEventArgs readEventArgs);

        public virtual void Dispose()
        {
            if (State <= SocketState.Disposed) return;

            Disconnect();

            WriteEventArgs.Dispose();
            ReadEventArgs.Dispose();
            State = SocketState.Disposed;
        }

        public virtual void Activate()
        {
            State = SocketState.Active;
            ReadRepeat();
        }
    }
}