using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    ///     Socket Wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DragonSocket<T> : IDragonSocket<T> where T : IMessage
    {
        protected readonly MessageConverter<T> MessageConverter;
        private readonly Queue<T> _sendingQueue = new Queue<T>();
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
        public virtual void Disconnect()
        {
            Socket.Close(1000);

            //run once
            if (State >= SocketState.Disconnected) return;

            State = SocketState.Disconnected;

            if (null != Disconnected)
                Disconnected();
        }

        public event MessageEventHandler<T> ReadCompleted
        {
            add { MessageConverter.MessageConverted += value; }
            remove { MessageConverter.MessageConverted -= value; }
        }

        public event MessageEventHandler<T> WriteCompleted;
        public event VoidMessageEventHandler Disconnected;

        protected Socket Socket { set; get; }
        private readonly SocketAsyncEventArgs _writeEventArgs;
        private readonly SocketAsyncEventArgs _readEventArgs;

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

        public void Send(T message)
        {
            lock (_lock)
            {
                _sendingQueue.Enqueue(message);

                if (_sending)
                {
                    return;
                }
                SendAsync(_sendingQueue.Dequeue());
            }
        }

        /// <summary>
        /// Should Run in lock
        /// </summary>
        /// <param name="message"></param>
        private void SendAsync(T message)
        {
            lock (_lock)
            {
                _sending = true;

                _writeEventArgs.SetBuffer(message.ToByteArray(), 0, message.Length);
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
                WriteCompleted((T) e.UserToken);

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
                MessageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
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