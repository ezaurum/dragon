using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Dragon
{
    /// <summary>
    ///     Client Socket. auto reconnect, concorrent
    ///     Has Request, Acknowlege templates
    /// </summary>
    public class ConcurrentClientDragonSocket<TReq, TAck> :
        ConcurrentDragonSocket<TReq, TAck>,
        IConnectable
    {
        private readonly Connector _connector;

        public event EventHandler<SocketAsyncEventArgs> ConnectFailed
        {
            add { _connector.ConnectFailed += value; }
            remove { _connector.ConnectFailed -= value; }
        }

        public event EventHandler<SocketAsyncEventArgs> ConnectSuccess
        {
            add { _connector.ConnectSuccess += value; }
            remove { _connector.ConnectSuccess -= value; }
        }

        public void Connect(IPEndPoint endPoint)
        {
            _connector.Connect(endPoint);
        }

        public void Connect(string ipAddress, int port)
        {
            _connector.Connect(ipAddress, port);
        }

        public ConcurrentClientDragonSocket(
            IMessageConverter<TReq, TAck> converter)
            : base(converter)
        {
            _connector = new Connector();
            ConnectSuccess += ActivateOnConnectSuccess;
        }

        private HeartBeatMaker<TReq> _heartBeatMaker;

        public bool HeartBeatEnable { get; set; }
        public TReq HeartBeatMessage { get; set; }

        public event Action<TReq> UpdateMessage
        {
            add { _heartBeatMaker.UpdateMessage += value; }
            remove { _heartBeatMaker.UpdateMessage -= value; }
        }

        public ConcurrentClientDragonSocket(
            IMessageConverter<TReq, TAck> converter,
            TReq beatMessage, int interval = 750) : this(converter)
        {
            InitHeartBeatMaker(beatMessage, interval);
        }

        private void InitHeartBeatMaker(TReq beatMessage, int interval)
        {
            HeartBeatEnable = true;
            HeartBeatMessage = beatMessage;
            _heartBeatMaker = new HeartBeatMaker<TReq>(this, HeartBeatMessage,
                interval);

            Disconnected += _heartBeatMaker.Stop;
            ConnectSuccess += _heartBeatMaker.Start;
        }

        private void ActivateOnConnectSuccess(object sender,
            SocketAsyncEventArgs e)
        {
            Socket = _connector.Socket;

            Activate();

            if (!HeartBeatEnable) return;
            _heartBeatMaker.Start();
        }
    }


    /// <summary>
    ///     Socket Wrapper, Request, Acknowledge packet devided. not inherit IMessage
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public abstract class ConcurrentDragonSocket<TReq, TAck> :
        ByteStreamSocketWrapper, IMessageSender<TReq>
    {
        private readonly IMessageConverter<TReq, TAck> _converter;

        private readonly ConcurrentQueue<TReq> _sendingQueue =
            new ConcurrentQueue<TReq>();

        private int _sendingMessage;

        protected ConcurrentDragonSocket(
            IMessageConverter<TReq, TAck> converter,
            byte[] buffer = null, int offset = 0, int bufferSize = 1024*16)
            : base(buffer ?? new byte[bufferSize], offset, bufferSize)
        {
            _converter = converter;
        }

        public event Action<int> WriteCompleted;

        public void Send(TReq message)
        {
            _sendingQueue.Enqueue(message);
            if (Interlocked.Increment(ref _sendingMessage) == 1)
            {
                SendAsyncFromQueue();
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

        public event Action<TAck, int> OnReadCompleted
        {
            add { _converter.MessageConverted += value; }
            remove { _converter.MessageConverted -= value; }
        }

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="args"></param>
        protected override void ReadEventCompleted(object socket,
            SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success) return;

            if (args.BytesTransferred < 1) return;

            _converter.Read(args.Buffer, args.Offset, args.BytesTransferred);

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