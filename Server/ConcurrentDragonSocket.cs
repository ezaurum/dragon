using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Dragon
{
    /// <summary>
    ///     Socket Wrapper, Request, Acknowledge packet devided. not inherit IMessage
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public abstract class ConcurrentDragonSocket<TReq, TAck> :
        ByteStreamSocketWrapper, IMessageSender<TReq>
    {
        private readonly ConcurrentQueue<TReq> _sendingQueue =
            new ConcurrentQueue<TReq>();

        protected long SendingMessage;

        protected ConcurrentDragonSocket(
            IMessageConverter<TReq, TAck> converter,
            byte[] buffer = null, int offset = 0, int bufferSize = 1024*16)
            : base(buffer ?? new byte[bufferSize], offset, bufferSize)
        {
            Converter = converter;
            OnReadCompleted += MessageConvert;
        }

        public event Action<int> WriteCompleted;

        public void Send(TReq message)
        {
            _sendingQueue.Enqueue(message);
            if (Interlocked.Increment(ref SendingMessage) == 1)
            {
                SendAsyncFromQueue();
            }
        }

        protected void SendAsyncFromQueue()
        {
            TReq message;
            if (_sendingQueue.TryPeek(out message))
                SendAsync(message); 
        }

        protected void SendAsync(TReq message)
        {
            byte[] messageBytes;
            int errorCode;
            Converter.GetByte(message, out messageBytes, out errorCode);
            if (0 != errorCode && null != WriteCompleted)
            {
                WriteCompleted(errorCode);
                return;
            }
            SendAsync(messageBytes);
        }

        public event Action<TAck, int> ReadCompleted
        {
            add { Converter.MessageConverted += value; }
            remove { Converter.MessageConverted -= value; }
        }

        protected override void WriteEventCompleted(object o,
            SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                if (null != WriteCompleted) WriteCompleted(-1);
                Disconnect(e);
                return;
            }
            if (null != WriteCompleted)
                WriteCompleted(0);

            //remove sended message
            TReq message;
            _sendingQueue.TryDequeue(out message);
            
            //if remaineded, send next
            if (Interlocked.Decrement(ref SendingMessage) > 0)
            {
                SendAsyncFromQueue();
            }
        }

        protected readonly IMessageConverter<TReq, TAck> Converter;

        public void MessageConvert(object socket, SocketAsyncEventArgs args) 
        {
            Converter.Read(args.Buffer, args.Offset, args.BytesTransferred);
        }
    }
}