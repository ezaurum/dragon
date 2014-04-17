using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

        private int _sendingMessage;

        private readonly Task _sendTask;

        protected ConcurrentDragonSocket(
            IMessageConverter<TReq, TAck> converter,
            byte[] buffer = null, int offset = 0, int bufferSize = 1024*16)
            : base(buffer ?? new byte[bufferSize], offset, bufferSize)
        {
            _converter = converter;
            _sendTask = new Task(SendAsyncFromQueue);
            OnReadCompleted += MessageConvert;
        }

        public event Action<int> WriteCompleted;

        public void Send(TReq message)
        {
            _sendingQueue.Enqueue(message);
            if (Interlocked.Increment(ref _sendingMessage) == 1)
            {
               _sendTask.Start(); 
            }
        }

        private void SendAsyncFromQueue()
        {
            TReq message;
            if (_sendingQueue.TryPeek(out message))
                SendAsync(message); 
        }

        private void SendAsync(TReq message)
        {
            byte[] messageBytes;
            int errorCode;
            _converter.GetByte(message, out messageBytes, out errorCode);
            if (0 != errorCode)
            {
                WriteCompleted(errorCode);
                return;
            }
            SendAsync(messageBytes);
        }

        public event Action<TAck, int> ReadCompleted
        {
            add { _converter.MessageConverted += value; }
            remove { _converter.MessageConverted -= value; }
        }

        protected override void WriteEventCompleted(object o,
            SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            if (null != WriteCompleted)
                WriteCompleted(0);

            //remove sended message
            TReq message;
            _sendingQueue.TryDequeue(out message);
            
            //if remaineded, send next
            if (Interlocked.Decrement(ref _sendingMessage) > 0)
            {
                SendAsyncFromQueue();
            }
        }

        private readonly IMessageConverter<TReq, TAck> _converter;

        public void MessageConvert(object socket, SocketAsyncEventArgs args) 
        {
            _converter.Read(args.Buffer, args.Offset, args.BytesTransferred);
        }
    }
}