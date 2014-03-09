using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Dragon
{
    #region one type socket
    /// <summary>
    ///     Socket Wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DragonSocket<T> : AbstractDragonSocket<T>, IDragonSocket<T> where T : IMessage
    {
        private readonly MessageConverter<T> _messageConverter;
        private readonly Queue<T> _sendingQueue = new Queue<T>();
        private readonly object _lock = new object();
        private bool _sending; 

        public DragonSocket(IMessageFactory<T> factory)
        { 
            _messageConverter = new MessageConverter<T>(factory); 
        }

        public event Action<T> ReadCompleted
        {
            add { _messageConverter.MessageConverted += value; }
            remove { _messageConverter.MessageConverted -= value; }
        } 

        public override void Send(T message)
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
            if (State != SocketState.Active) return;

            _sending = true;
            WriteEventArgs.UserToken = message;
            try
            {
                WriteEventArgs.SetBuffer(message.ToByteArray(), 0, message.Length);
                
                if (Socket.SendAsync(WriteEventArgs)) return;
                OnWriteEventArgsOnCompleted(Socket, WriteEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                if (State == SocketState.Active)
                {
                    throw new InvalidOperationException("Socket State is Active. But socket disposed.", e);
                }
            }
        } 

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="args"></param>
        protected override void OnReadEventArgsOnCompleted(object socket, SocketAsyncEventArgs args)
        {
            if (SocketError.Success != args.SocketError || SocketState.Active != State)
                return;

            _messageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
            ReadRepeat();
        }

        public event Action WriteCompleted;

        protected override void OnWriteEventArgsOnCompleted(object socket,
            SocketAsyncEventArgs writeEventArgs)
        {
            switch (writeEventArgs.SocketError)
            {
                case SocketError.Success:

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
                    break;

                case SocketError.ConnectionReset:
                case SocketError.Disconnecting:
                case SocketError.NotConnected:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        string.Format("writeEventArgs.SocketError {0}",
                            writeEventArgs));
            }
        }
    }
    #endregion

    #region req/ack separated type message

    /// <summary>
    ///     Socket Wrapper, Request, Acknowledge packet devided. not inherit IMessage
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public class DragonSocket<TReq, TAck> : AbstractDragonSocket<TReq>,IDragonSocket<TReq, TAck>
    {
        private readonly IMessageFactory<TReq, TAck> _factory;
        private readonly Queue<TReq> _sendingQueue = new Queue<TReq>();
        private readonly object _lock = new object();
        private bool _sending;

        protected DragonSocket(IMessageFactory<TReq, TAck> factory, byte[] buffer, int index, int length)
        {
            _factory = factory;
            _buffer = buffer;
            _offset = index;
            _initialOffset = index;
            _bufferLength = length;
        }

        protected DragonSocket(IMessageFactory<TReq, TAck> factory)
        {
            _factory = factory; 
        }

        public event Action<TAck, int> ReadCompleted;
        public event Action<int> WriteCompleted;

        public override void Send(TReq message)
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
                WriteEventArgs.UserToken = errorCode;
                if (0 != errorCode)
                {
                    OnWriteEventArgsOnCompleted(Socket, WriteEventArgs);
                    return;
                }

                WriteEventArgs.SetBuffer(messageBytes, 0, messageBytes.Length);
            }
            try
            {
                if (Socket.SendAsync(WriteEventArgs)) return;
                OnWriteEventArgsOnCompleted(Socket, WriteEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                if (State == SocketState.Active)
                {
                    throw new InvalidOperationException("Socket State is Active. But socket disposed.", e);
                }
            }

        }

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="args"></param>
        protected override void OnReadEventArgsOnCompleted(object socket, SocketAsyncEventArgs args)
        { 
            if (args.SocketError != SocketError.Success)
            {
                Disconnect(args);
                return;
            }
            if (SocketError.Success != args.SocketError || 0 >= args.BytesTransferred || SocketState.Active != State)
                return;
            ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
            ReadRepeat();
        }
        
        protected override void OnWriteEventArgsOnCompleted(object o, SocketAsyncEventArgs e)
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

        private readonly byte[] _buffer;

        private int _offset;
        private readonly int _bufferLength;
        private int _initialOffset;

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

            if (_offset > _bufferLength) throw new InvalidDataException("buffer overflow " + _offset + "/"+_bufferLength);

            while (_offset > 2 )
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
    }

    #endregion
}