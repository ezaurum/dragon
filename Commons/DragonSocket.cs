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
        private readonly MessageConverter<T> _messageConverter;
        private readonly Queue<T> _sendingQueue = new Queue<T>();
        private readonly object _lock = new object();
        private bool _sending;
        private bool _leftToSend;

        public enum SocketState
        {
            BeforeInitialized,
            Initialized,
            Active,
            Inactive
        }

        public SocketState State { get; set; }

        protected DragonSocket(IMessageFactory<T> factory)
        {
            State = SocketState.BeforeInitialized;

            //TODO buffer reallocated
            _messageConverter = new MessageConverter<T>(factory);
            ReadEventArgs = new SocketAsyncEventArgs();

            //TODO is this need pool?
            ReadEventArgs.SetBuffer(new byte[1024], 0, 1024);
            ReadEventArgs.Completed += OnReadEventArgsOnCompleted;

            WriteEventArgs = new SocketAsyncEventArgs();
            WriteEventArgs.Completed += OnWriteEventArgsOnCompleted;

            State = SocketState.Initialized;
        }


        private void Disconnect(object sender, SocketAsyncEventArgs e)
        {
            ReadEventArgs.Dispose();
            WriteEventArgs.Dispose();
            Socket.Close();

            //run once
            if (State == SocketState.Inactive) return;

            State = SocketState.Inactive;

            if (null != Disconnected)
                Disconnected(sender, e);
        }

        public event MessageEventHandler<T> ReadCompleted
        {
            add { _messageConverter.MessageConverted += value; }
            remove { _messageConverter.MessageConverted -= value; }
        }

        public event MessageEventHandler<T> WriteCompleted;
        public event EventHandler<SocketAsyncEventArgs> Disconnected;

        protected Socket Socket { set; get; }
        private SocketAsyncEventArgs WriteEventArgs { set; get; }
        private SocketAsyncEventArgs ReadEventArgs { set; get; }

        public void Activate()
        {
            State = SocketState.Active;
            ReadRepeat();
        }

        public void Deactivate()
        {
            Disconnect(Socket,null);
        }

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
            
            lock (_lock)
            {
                SendAsync(_sendingQueue.Dequeue());
            }
        }

        /// <summary>
        /// Should Run in lock
        /// </summary>
        /// <param name="message"></param>
        private void SendAsync(T message)
        {
            _sending = true;

            WriteEventArgs.SetBuffer(message.ToByteArray(), 0, message.Length);
            WriteEventArgs.UserToken = message;

            if (!Socket.SendAsync(WriteEventArgs))
            {
                OnWriteEventArgsOnCompleted(null, WriteEventArgs);
            }
        }

        private void OnWriteEventArgsOnCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Disconnect(sender, e);
                return;
            }

            if ( null != WriteCompleted)
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
                if (!Socket.ReceiveAsync(ReadEventArgs))
                {
                    OnReadEventArgsOnCompleted(Socket, ReadEventArgs);
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
                Disconnect(sender,args);
                return;
            }

            if (SocketError.Success == args.SocketError && 0 < args.BytesTransferred && SocketState.Active == State)
            {
                _messageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
                ReadRepeat();
            }
        }
    }
}