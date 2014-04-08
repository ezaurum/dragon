using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Dragon
{
    /// <summary>
    ///     Socket Wrapper, Request, Acknowledge packet devided. not inherit IMessage
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public abstract class DragonSocket<TReq, TAck> : ByteStreamSocketWrapper, IMessageSender<TReq>
    {
        protected readonly IMessageConverter<TReq, TAck> Converter;
        private readonly Queue<TReq> _sendingQueue = new Queue<TReq>();
        private readonly object _lock = new object();


        private int _sendingMessages;

        protected DragonSocket(IMessageConverter<TReq, TAck> converter,
            byte[] buffer = null, int offset = 0, int bufferSize = 1024*16)
            : base(buffer ?? new byte[bufferSize], offset, bufferSize)
        {
            Converter = converter; 
        }
        
        public event Action<int> WriteCompleted;

        public void Send(TReq message)
        {
            if (Interlocked.Increment(ref _sendingMessages) > 1)
            {
                lock (_lock)
                {
                    _sendingQueue.Enqueue(message);
                }
                return;
            }

            SendAsync(message);
        }

        private void SendAsyncFromQueue()
        {
            TReq message;
            lock (_lock)
            {
                message = _sendingQueue.Dequeue();
            }

            SendAsync(message);
        }

        private void SendAsync(TReq message)
        {
            byte[] messageBytes;
            int errorCode;
            Converter.GetByte(message, out messageBytes, out errorCode);
            if (0 != errorCode)
            {
                WriteCompleted(errorCode);
                return;
            }

            SendAsync(messageBytes);
        }

        protected override void WriteEventCompleted(object o, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            if (null != WriteCompleted)
                WriteCompleted(0);

            if (Interlocked.Decrement(ref _sendingMessages) < 1) return;

            SendAsyncFromQueue(); 
        }

        public virtual event Action<TAck,int> OnReadCompleted
        {
            add { Converter.MessageConverted += value; }
            remove { Converter.MessageConverted -= value; }
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

            Converter.Read(args.Buffer, args.Offset, args.BytesTransferred);

            try
            {
                args.SetBuffer(args.Offset, args.Count);
            }
            catch (ObjectDisposedException ex)
            {
                //ignore?
                Disconnect();
                return;
            }

            ReadRepeat();
        }
    }
}