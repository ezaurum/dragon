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
    public abstract class DragonSocket<TReq, TAck> : ByteStreamSocketWrapper, IMessageSender<TReq>
    {
        protected readonly IMessageConverter<TReq, TAck> _converter;
        private readonly Queue<TReq> _sendingQueue = new Queue<TReq>();
        private readonly object _lock = new object();

        private bool _sending;

        protected DragonSocket(IMessageConverter<TReq, TAck> converter,
            byte[] buffer = null, int offset = 0, int bufferSize = 1024*16)
            : base(buffer ?? new byte[bufferSize], offset, bufferSize)
        {
            _converter = converter; 
        }
        
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
            TReq message;

            lock (_lock)
            {
                message = _sendingQueue.Dequeue();
            }

            byte[] messageBytes;
            int errorCode;
            _converter.GetByte(message, out messageBytes, out errorCode);
            if (0 != errorCode)
            {
                WriteCompleted(errorCode);
                return;
            }
            
            lock (_lock)
            {
                _sending = true;
                SendAsync(messageBytes);
            }
        } 
        
        protected override void WriteEventCompleted(object o, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            if (null != WriteCompleted)
                WriteCompleted(0);

            lock (_lock)
            {
                _sending = _sendingQueue.Count > 0;
            }

            if (!_sending) return;

            SendAsyncFromQueue();

        }

        public virtual event Action<TAck, int> OnReadCompleted
        {
            add { _converter.MessageConverted += value; }
            remove { _converter.MessageConverted -= value; }
        }

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="args"></param>
        protected override void ReadEventCompleted(object socket, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success) return;

            if (args.BytesTransferred < 1) return;

            _converter.Read(args.Buffer, args.Offset, args.BytesTransferred);

            args.SetBuffer(args.Offset,args.Count);

            ReadRepeat();
        }
    }
}