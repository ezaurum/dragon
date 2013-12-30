using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    ///     Socket Wrapper, Request, Acknowledge packet devided. not inherit IMessage
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public class DragonSocket<TReq, TAck> : EndPointStorage, IDragonSocket<TReq, TAck>
    {
        private readonly IMessageFactory<TReq, TAck> _factory;
        private readonly Queue<TReq> _sendingQueue = new Queue<TReq>();
        private readonly object _lock = new object();
        private bool _sending;
        private bool _leftToSend;

        public enum SocketState
        {
            BeforeInitialized = 0,
            Initialized,
            Active = 10,
            Inactive = 100,
            Disconnected
        }

        public SocketState State { get; set; }

        protected DragonSocket(IMessageFactory<TReq, TAck> factory)
        {
            _factory = factory;
            State = SocketState.BeforeInitialized;

            //TODO buffer reallocated
            
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
        public virtual void Disconnect()
        {
            Socket.Close(1000);

            //run once
            if (State >= SocketState.Disconnected) return;

            State = SocketState.Disconnected;

            if (null != Disconnected)
                Disconnected();
        }

        public event MessageHandler<TReq> WriteCompleted;

        public event MessageHandler<TAck> ReadCompleted;
        
        public event VoidMessageEventHandler Disconnected;

        protected Socket Socket { set; get; }
        private readonly SocketAsyncEventArgs _writeEventArgs;
        private readonly SocketAsyncEventArgs _readEventArgs;
        private byte[] _sendingBytes;

        public virtual void Activate()
        {
            State = SocketState.Active;
            ReadRepeat();
        }

        public void Deactivate()
        {
            State = SocketState.Inactive;
            Disconnect();
        }

        public void Send(TReq message)
        {
            lock (_lock)
            {
                _sendingQueue.Enqueue(message);

                if (_sending)
                {
                    return;
                }
                _factory.GetByte(_sendingQueue.Dequeue(), out _sendingBytes);
                SendAsync(_sendingBytes);
            }
        }

        /// <summary>
        /// Should Run in lock
        /// </summary>
        /// <param name="message"></param>
        private void SendAsync(byte[] message)
        {
            lock (_lock)
            {
                _sending = true;
                _writeEventArgs.SetBuffer(message, 0, message.Length);
                _writeEventArgs.UserToken = message;
            }
            try
            {
                if (!Socket.SendAsync(_writeEventArgs))
                {
                    OnWriteEventArgsOnCompleted(null, _writeEventArgs);
                }
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
                Disconnect();
                return;
            }

            if (null != WriteCompleted)
                WriteCompleted((TReq)e.UserToken);

            lock (_lock)
            {
                _sending = _sendingQueue.Count > 0;
            }

            if (_sending)
            {
                _factory.GetByte(_sendingQueue.Dequeue(), out _sendingBytes);
                SendAsync(_sendingBytes);
            }
        }

        /// <summary>
        ///     Read repeat
        /// </summary>
        private void ReadRepeat()
        {
            try
            {
                if (!Socket.ReceiveAsync(_readEventArgs))
                {
                    OnReadEventArgsOnCompleted(Socket, _readEventArgs);
                }
            }
            catch (ObjectDisposedException e)
            {
                //Nothing to do
            }
        }

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            //TODO error process need
            if (args.SocketError != SocketError.Success)
            {
                Disconnect();
                return;
            }

            if (SocketError.Success == args.SocketError && 0 < args.BytesTransferred && SocketState.Active == State)
            {
                //                _messageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
                ReadRepeat();
            }
        }

        public virtual void Dispose()
        {
            if (State >= SocketState.Inactive) return;

            _writeEventArgs.Dispose();
            _readEventArgs.Dispose();

            Deactivate();
        }
    }
}