using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Dragon
{
    #region one type socket
    /// <summary>
    ///     Socket Wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DragonSocket<T> : IDragonSocket<T> where T : IMessage
    {
        private readonly MessageConverter<T> MessageConverter;
        private readonly Queue<T> _sendingQueue = new Queue<T>();
        private readonly object _lock = new object();
        private bool _sending;

        public enum SocketState
        {
            BeforeInitialized = 0,
            Initialized,
            Active = 10,
            Inactive = 100,
            Disconnected
        }

        public SocketState State { get; set; }

        public EndPoint RemoteEndPoint
        {
            get { return Socket.RemoteEndPoint; } 
        }

        public EndPoint LocalEndPoint
        {
            get { return Socket.LocalEndPoint; }
        }

        protected DragonSocket(IMessageFactory<T> factory)
        {
            State = SocketState.BeforeInitialized;

            //TODO buffer reallocated
            MessageConverter = new MessageConverter<T>(factory);
            _readEventArgs = new SocketAsyncEventArgs();

            //TODO is this need pool?
            _readEventArgs.SetBuffer(new byte[1024], 0, 1024);
            _readEventArgs.Completed += OnReadEventArgsOnCompleted;

            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += OnWriteEventArgsOnCompleted;

            State = SocketState.Initialized;

        }

        /// <summary>
        /// For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect()
        {
            Disconnect(null); 
        }

        /// <summary>
        /// For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect(SocketAsyncEventArgs e)
        {
            //run once
            if (State >= SocketState.Disconnected) return;
            State = SocketState.Disconnected;

            if (Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Disconnect(true);
            }

            if (null != OnDisconnected)
                OnDisconnected(this,e);
        }

        public event Action<T> ReadCompleted
        {
            add { MessageConverter.MessageConverted += value; }
            remove { MessageConverter.MessageConverted -= value; }
        }

        public event Action WriteCompleted; 

        protected Socket Socket { set; get; }
        private readonly SocketAsyncEventArgs _writeEventArgs;
        private readonly SocketAsyncEventArgs _readEventArgs;

        public virtual void Activate()
        {
            State = SocketState.Active;
            ReadRepeat();
        }


        public event EventHandler<SocketAsyncEventArgs> OnDisconnected;

        public void Send(T message)
        {
            lock (_lock)
            {
                _sendingQueue.Enqueue(message);
            }

            if (_sending)
            {
                return;
            }
            SendAsync(_sendingQueue.Dequeue());
            
        }

        /// <summary>
        /// Should Run in lock
        /// </summary>
        /// <param name="message"></param>
        private void SendAsync(T message)
        {
            _sending = true;
            _writeEventArgs.UserToken = message;
            try
            {
                _writeEventArgs.SetBuffer(message.ToByteArray(), 0, message.Length);
                
                if (Socket.SendAsync(_writeEventArgs)) return;
                OnWriteEventArgsOnCompleted(Socket, _writeEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                if (State == SocketState.Active)
                {
                    throw new InvalidOperationException("Socket State is Active. But socket disposed.", e);
                }
            }

        }

        private void OnWriteEventArgsOnCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Disconnect(e);
                return;
            }

            if (null != WriteCompleted)
                WriteCompleted();

            lock (_lock)
            {
                _sending = _sendingQueue.Count > 0;
            }

            if (_sending)
            {
                SendAsync(_sendingQueue.Dequeue());
            }
        }

        /// <summary>
        ///     Read repeat
        /// </summary>
        private void ReadRepeat()
        {
            try
            {
                if (Socket.ReceiveAsync(_readEventArgs)) return;
                OnReadEventArgsOnCompleted(Socket, _readEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                Disconnect();
                if (State == SocketState.Active)
                {
                    throw new InvalidOperationException("Socket State is Active. But socket disposed.", e);
                }
            }
        }

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                Disconnect(args);
                return;
            }
            if (SocketError.Success != args.SocketError || 0 >= args.BytesTransferred || SocketState.Active != State)
                return;
            MessageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
            ReadRepeat();
        }

        public virtual void Dispose()
        {
            if (State >= SocketState.Inactive) return;

            Disconnect();

            State = SocketState.Inactive;

            _writeEventArgs.Dispose();
            _readEventArgs.Dispose(); 
        }
    }
    #endregion

    #region req/ack separated type message

    /// <summary>
    ///     Socket Wrapper, Request, Acknowledge packet devided. not inherit IMessage
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public class DragonSocket<TReq, TAck> : IDragonSocket<TReq, TAck>
    {
        private readonly IMessageFactory<TReq, TAck> _factory;
        private readonly Queue<TReq> _sendingQueue = new Queue<TReq>();
        private readonly object _lock = new object();
        private bool _sending;

        public enum SocketState
        {
            BeforeInitialized = 0,
            Initialized,
            Active = 10,
            Inactive = 100,
            Disconnected
        }

        public SocketState State { get; set; }

        protected DragonSocket(IMessageFactory<TReq, TAck> factory, byte[] buffer, int index, int length)
        {
            _factory = factory;
            State = SocketState.BeforeInitialized;

            //TODO buffer reallocated

            _buffer = new byte[2048];

            _readEventArgs = new SocketAsyncEventArgs();

            _readEventArgs.SetBuffer(buffer, index, length);
            _readEventArgs.Completed += OnReadEventArgsOnCompleted;

            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += OnWriteEventArgsOnCompleted;

            State = SocketState.Initialized;
        }

        /// <summary>
        /// For reuse, Socket and eventargs are not disposed.
        /// </summary>
        public void Disconnect(SocketAsyncEventArgs e = null)
        {
            Socket.Close(1000);

            //run once
            if (State >= SocketState.Disconnected) return;

            State = SocketState.Disconnected;

            if (null != OnDisconnected)
            {
                OnDisconnected(this,e);
            }
        }

        public event EventHandler<SocketAsyncEventArgs> OnDisconnected;

        protected Socket Socket { set; get; }
        private readonly SocketAsyncEventArgs _writeEventArgs;
        private readonly SocketAsyncEventArgs _readEventArgs;
        
        public virtual void Activate()
        {
            State = SocketState.Active;
            ReadRepeat();
        }

        public event Action<TAck, int> ReadCompleted;
        public event Action<int> WriteCompleted;

        public void Send(TReq message)
        {
            lock (_lock)
            {
                _sendingQueue.Enqueue(message);

                if (_sending)
                {
                    return;
                }
                SendAsyncFromQueue();
            }
        }

        private void SendAsyncFromQueue()
        {
            SendAsync(_sendingQueue.Dequeue());
        }

        /// <summary>
        /// Should Run in lock 
        /// </summary>
        /// <param name="message"></param>
        private void SendAsync(TReq message)
        {
            lock (_lock)
            {
                _sending = true;
                byte[] messageBytes;
                int errorCode;
                _factory.GetByte(message, out messageBytes, out errorCode);
                _writeEventArgs.UserToken = errorCode;
                if (0 != errorCode)
                {
                    OnWriteEventArgsOnCompleted(Socket, _writeEventArgs);
                    return;
                }

                _writeEventArgs.SetBuffer(messageBytes, 0, messageBytes.Length);
            }
            try
            {
                if (Socket.SendAsync(_writeEventArgs)) return;
                OnWriteEventArgsOnCompleted(Socket, _writeEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                if (State == SocketState.Active)
                {
                    throw new InvalidOperationException("Socket State is Active. But socket disposed.", e);
                }
            }

        }

        private void OnWriteEventArgsOnCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Disconnect(e);
                return;
            }

            if (null != WriteCompleted)
                WriteCompleted((int)e.UserToken);

            lock (_lock)
            {
                _sending = _sendingQueue.Count > 0;
            }

            if (!_sending) return;

            SendAsyncFromQueue();
        }

        /// <summary>
        ///     Read repeat
        /// </summary>
        private void ReadRepeat()
        {
            try
            {
                if (Socket.ReceiveAsync(_readEventArgs)) return;
                OnReadEventArgsOnCompleted(Socket, _readEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                if (State == SocketState.Active)
                {
                    throw new InvalidOperationException("Socket State is Active. But socket disposed.", e);
                }
            }
        }

        private readonly byte[] _buffer;

        private int _offset;


        private void PullBufferToFront(short messageLength)
        {
            Buffer.BlockCopy(_buffer, messageLength, _buffer, 0, _offset - messageLength);
            _offset -= messageLength;
        }

        private void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {
            Buffer.BlockCopy(buffer, offset, _buffer, _offset, bytesTransferred);

            //add offset
            _offset += bytesTransferred;

            while (_offset > 2)
            {
                short messageLength = BitConverter.ToInt16(_buffer, 0);

                if (_offset < messageLength) return;

                TAck tack;
                int errorCode;
                _factory.GetMessage(_buffer, 0, messageLength, out tack, out errorCode);
                PullBufferToFront(messageLength);
                ReadCompleted(tack, errorCode);
            }
        }

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            switch (args.SocketError)
            {
                case SocketError.Success:
                    if (SocketError.Success == args.SocketError
                        && 0 < args.BytesTransferred
                        && SocketState.Active == State)
                    {
                        ReceiveBytes(args.Buffer, 0, args.BytesTransferred);
                        ReadRepeat();
                    }
                    break;
                default:
                    Disconnect(args);
                    return;
            }
        }

        public virtual void Dispose()
        {
            if (State >= SocketState.Inactive) return;

            Disconnect();

            State = SocketState.Inactive;

            _writeEventArgs.Dispose();
            _readEventArgs.Dispose();
        }
    }
    #endregion
}