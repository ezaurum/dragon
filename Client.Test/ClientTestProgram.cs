using System;
using System.Net.Sockets;
using Dragon;
using Dragon.Client;

namespace Dragon
{
    

    public class ConnectSession<T> : ISession<T>
    {


        /// <summary>
        /// session activate. start to read/write
        /// </summary>
        public void Activate()
        {
            MessageConverter.Session = this;

            if (null != Provided)
            {
                Provided(this);
            }

            ReadEventArgs.Completed += OnReadEventArgsOnCompleted;
            WriteEventArgs.Completed += OnWriteEventArgsOnCompleted;

            MessageConverter.MessageConvertCompleted += InvokeReaction;

            //TODO allocate by buffer provider
            ReadEventArgs.SetBuffer(new byte[1024], 0, 1024);

            //start read
            Read();
        }

        private void OnWriteEventArgsOnCompleted(object sender, SocketAsyncEventArgs e)
        {

        }

        private void InvokeReaction(DragonMarbleGameSession session, IDragonMarbleGameMessage message)
        {
            if (MessageReactions.ContainsKey(message.MessageType))
            {
                //in message reactions, session neeed.
                MessageReactions[message.MessageType](session, message);
            }
            else if (null != StageUnit)
            {
                StageUnit.MessageProcessor.ReceivedMessage = message;
            }
        }




        //TODO can move?
        private void Read()
        {
            if (!Socket.ReceiveAsync(ReadEventArgs))
            {
                OnReadEventArgsOnCompleted(Socket, ReadEventArgs);
            }
        }

    }
}

namespace Dragon.Client
{
    public class ClientNetworkManager<T> where T : IMessage
    {
        private SocketConnector _connector;

        public ISession<T> Session { get; set; }
        
        public SocketAsyncEventArgs ConnectionEventArgs { get; set; }

        public ClientNetworkManager()
        {
            _connector = new SocketConnector
            {
                ConnectEventArgs = socketAsyncEventArgs
            };

            SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.Completed += MakeSession;
        }        


        

        private void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            //TODO error process need
            if (args.SocketError != SocketError.Success) return;

            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                MessageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
                Read();
            }
        }

        private readonly Queue<IDragonMarbleGameMessage> _sendingQueue = new Queue<IDragonMarbleGameMessage>();
        private readonly object _lock = new object();

        public void SendMessage(DragonMarbleGameSession session, IDragonMarbleGameMessage message)
        {
            lock (_lock)
            {
                _sendingQueue.Enqueue(message);
            }

            if (_sending)
            {
                return;
            }

            _sending = true;
            while (_sendingQueue.Count > 0)
            {
                IDragonMarbleGameMessage dragonMarbleGameMessage;
                lock (_lock)
                {
                    dragonMarbleGameMessage = _sendingQueue.Dequeue();
                }
                Write(dragonMarbleGameMessage);
            }
            _sending = false;
        }

        private void Write(IDragonMarbleGameMessage dragonMarbleGameMessage)
        {
            WriteEventArgs.SetBuffer(dragonMarbleGameMessage.ToByteArray(), 0, dragonMarbleGameMessage.Length);
            if (!Socket.SendAsync(WriteEventArgs))
            {
                OnWriteEventArgsOnCompleted(Socket, WriteEventArgs);
            }
        }

        
        
        private void MakeSession(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;

            SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs();
            readEventArgs.Completed += (o, args) =>
            {
                if (args.BytesTransferred > 0
                    && args.SocketError == SocketError.Success)
                {
                    ((Socket)o).ReceiveAsync(args);
                }
            };

            SocketAsyncEventArgs writeEventArgs = new SocketAsyncEventArgs();
            writeEventArgs.Completed += (o, args) =>
            {
                if (args.SocketError == SocketError.Success)
                {

                }
            };

            DummySession ds = new DummySession
            {
                Socket = e.AcceptSocket,
                ReadEventArgs = readEventArgs,
                WriteEventArgs = writeEventArgs
            };

            //start read
        }


    }    
}

namespace Client.Test
{
    public static class ClientTestProgram
    {
        static void Main(string[] args)
        {
            SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.Completed += MakeSession;
            
            SocketConnector c = new SocketConnector
            {
                ConnectEventArgs = socketAsyncEventArgs
            };

            c.Connect("127.0.0.1",10008);            
            
            Console.ReadKey();
        }

        

        private static void MakeSession(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;

            SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs();
            readEventArgs.Completed += (o, args) =>
            {
                if (args.BytesTransferred > 0 
                    && args.SocketError == SocketError.Success)
                {
                    ((Socket)o).ReceiveAsync(args);
                }
            };
            
            SocketAsyncEventArgs writeEventArgs = new SocketAsyncEventArgs();
            writeEventArgs.Completed += (o, args) =>
            {
                if (args.SocketError == SocketError.Success)
                {
                    
                }
            };
            
            DummySession ds = new DummySession
            {
                Socket = e.AcceptSocket,
                ReadEventArgs = readEventArgs,
                WriteEventArgs = writeEventArgs
            };

            //start read
        }
    }

    public delegate void SessionEventHandler(DragonMarbleGameSession session);

    public class DragonMarbleGameSession : ISession<DummyMessage>
    {       
        public Dictionary<GameMessageType, GameEventHandler> MessageReactions
        {
            get { return _messageReactions; }
            set
            {
                Interlocked.Exchange(ref _messageReactions, value);
            }
        }

        public MessageConverter MessageConverter { private get; set; }

        private DateTime _lastLogin;
        private Dictionary<GameMessageType, GameEventHandler> _messageReactions;
        private bool _sending;

        public Guid GameRoomId { get; set; }

        public bool IsPlaying
        {
            get
            {
                return Guid.Empty != GameRoomId;
            }
        }

        public DateTime LastLogin
        {
            get { return _lastLogin; }
            set
            {
                _lastLogin = value;
                ExpireTime = _lastLogin;
                ExpireTime = value + SessionExpire;
            }
        }

        public DateTime ExpireTime { get; private set; }

        public bool IsLoggedIn
        {
            get
            {
                return !(IsExpired || Guid.Empty == Id);
            }
        }

        public bool IsExpired
        {
            get { return ExpireTime > DateTime.Now; }
        }

        public GameAccountInfo GameAccount { get; set; }

        public void Dispose()
        {
            GameAccount = null;
            GameRoomId = Guid.Empty;
        }

        public Guid Id { get; set; }

        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadEventArgs { get; set; }
        public SocketAsyncEventArgs WriteEventArgs { get; set; }

        public IMessageProcessor<IDragonMarbleGameMessage> MessageProcessor { get; set; }

        public bool IsRoomOwner
        {
            get { return StageUnit.IsRoomOwner; }
        }

        public StageUnitInfo StageUnit { get; set; }

        public event SessionEventHandler Provided;

        /// <summary>
        /// session activate. start to read/write
        /// </summary>
        public void Activate()
        {
            MessageConverter.Session = this;

            if (null != Provided)
            {
                Provided(this);
            }

            ReadEventArgs.Completed += OnReadEventArgsOnCompleted;
            WriteEventArgs.Completed += OnWriteEventArgsOnCompleted;

            MessageConverter.MessageConvertCompleted += InvokeReaction;

            //TODO allocate by buffer provider
            ReadEventArgs.SetBuffer(new byte[1024], 0, 1024);

            //start read
            Read();
        }

        private void OnWriteEventArgsOnCompleted(object sender, SocketAsyncEventArgs e)
        {

        }

        //session can be remove?
        private void InvokeReaction(DragonMarbleGameSession session, IDragonMarbleGameMessage message)
        {
            if (MessageReactions.ContainsKey(message.MessageType))
            {
                //in message reactions, session neeed.
                MessageReactions[message.MessageType](session, message);
            }
            else if (null != StageUnit)
            {
                StageUnit.MessageProcessor.ReceivedMessage = message;
            }
        }

        //TODO can move?
        private void Read()
        {
            if (!Socket.ReceiveAsync(ReadEventArgs))
            {
                OnReadEventArgsOnCompleted(Socket, ReadEventArgs);
            }
        }

        private void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            //TODO error process need
            if (args.SocketError != SocketError.Success) return;

            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                MessageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
                Read();
            }
        }

        private readonly Queue<IDragonMarbleGameMessage> _sendingQueue = new Queue<IDragonMarbleGameMessage>();
        private readonly object _lock = new object();

        public void SendMessage(DragonMarbleGameSession session, IDragonMarbleGameMessage message)
        {
            lock (_lock)
            {
                _sendingQueue.Enqueue(message);
            }

            if (_sending)
            {
                return;
            }

            _sending = true;
            while (_sendingQueue.Count > 0)
            {
                IDragonMarbleGameMessage dragonMarbleGameMessage;
                lock (_lock)
                {
                    dragonMarbleGameMessage = _sendingQueue.Dequeue();
                }
                Write(dragonMarbleGameMessage);
            }
            _sending = false;
        }

        private void Write(IDragonMarbleGameMessage dragonMarbleGameMessage)
        {
            WriteEventArgs.SetBuffer(dragonMarbleGameMessage.ToByteArray(), 0, dragonMarbleGameMessage.Length);
            if (!Socket.SendAsync(WriteEventArgs))
            {
                OnWriteEventArgsOnCompleted(Socket, WriteEventArgs);
            }
        }
    }

    public class DummySession : ISession<DummyMessage>
    {
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadEventArgs { get; set; }
        public SocketAsyncEventArgs WriteEventArgs { get; set; }
        public IMessageProcessor<DummyMessage> MessageProcessor { get; set; }
        public event EventHandler<MessageAsyncEventArgs<DummyMessage>> OnMessageSent;
        public event EventHandler<MessageAsyncEventArgs<DummyMessage>> OnMessageConverted;
        public event EventHandler<SocketAsyncEventArgs> OnSendCompleted;
        public event EventHandler<SocketAsyncEventArgs> OnReceiveCompleted;

        public void StartRead()
        {
            if (!Socket.ReceiveAsync(ReadEventArgs))
            {
                OnReceiveCompleted(Socket, ReadEventArgs);
            }
        }
    }

    public class SimpleSession<T> : ISession<T> where T : IMessage
    {
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadEventArgs { get; set; }
        public SocketAsyncEventArgs WriteEventArgs { get; set; }
        public IMessageProcessor<T> MessageProcessor { get; set; }
        public event EventHandler<MessageAsyncEventArgs<T>> OnMessageSent;
        public event EventHandler<MessageAsyncEventArgs<T>> OnMessageConverted;
        public event EventHandler<SocketAsyncEventArgs> OnSendCompleted;
        public event EventHandler<SocketAsyncEventArgs> OnReceiveCompleted;

        public SimpleSession(SocketAsyncEventArgs readArgs, SocketAsyncEventArgs writeArgs)
        {
            ReadEventArgs = readArgs;
            WriteEventArgs = writeArgs;
            OnReceiveCompleted += ReadCheck;
            ReadEventArgs.Completed += OnReceiveCompleted;
        }

        private void ReadCheck(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 
                && args.SocketError == SocketError.Success)
            {
                ReadAsyncContinually();

                MessageAsyncEventArgs<T> messageAsyncEventArgs = new MessageAsyncEventArgs<T>();
                //TODO some converting method
                OnMessageConverted(this, messageAsyncEventArgs);
            }
        }

        public void ReadAsyncContinually()
        {
            if (!Socket.ReceiveAsync(ReadEventArgs))
            {
                OnReceiveCompleted(Socket, ReadEventArgs);
            }
        }
    }

    public class DummyMessage : IMessage
    {
        public short Length
        {
            get { return 8; }
        }

        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(Int64.MaxValue);
        }

        public void FromByteArray(byte[] bytes)
        {
            
        }

        public DateTime PacketTime { get; set; }
    }    
}
