using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Dragon.Interfaces;
using log4net;

namespace Dragon.Server
{
    // user token for async process.
    public class QueuedMessageProcessor : IAsyncUserToken
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id { get { return _id; } }
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueuedMessageProcessor));
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadArg { get; set; }
        public SocketAsyncEventArgs WriteArg { get; set; }

        private readonly Queue<IGameMessage> _receivedMessages = new Queue<IGameMessage>();
        private readonly Queue<IGameMessage> _sendingMessages = new Queue<IGameMessage>();

        private readonly EventWaitHandle _receiveMessageWaitHandler = new ManualResetEvent(false);
        private readonly EventWaitHandle _sendMessageWaitHandler = new ManualResetEvent(false);

        public IEnumerable<IGameMessage> ReceivedMessages
        {
            set
            {
                foreach (IGameMessage gameMessage in value)
                {
                    ReceivedMessage = gameMessage;
                }
            }
        }

        public virtual  IGameMessage ReceivedMessage
        {
            get 
            {   
                if (_receivedMessages.Count < 1)
                {
                    _receiveMessageWaitHandler.Reset();
                    Logger.Debug("Reset received message");
                    Logger.DebugFormat("{0}", Player.GetType());
                }
                Logger.DebugFormat("Start waiting. {0}",_receivedMessages.Count);
                _receiveMessageWaitHandler.WaitOne();
                Logger.Debug("Deque.");
                return _receivedMessages.Dequeue();
            }
            set
            {
                _receivedMessages.Enqueue(value);
                Logger.DebugFormat("received message. {0}", _receivedMessages.Count);
                _receiveMessageWaitHandler.Set();
            }
        }

        public virtual IGameMessage SendingMessage
        {
            get
            {
                if (_sendingMessages.Count < 1)
                {
                    _sendMessageWaitHandler.Reset();
                }
                _sendMessageWaitHandler.WaitOne();
                return _sendingMessages.Dequeue();
            }
            set
            {
                _sendingMessages.Enqueue(value);
                _sendMessageWaitHandler.Set();
            }
        }

        public object Player { get; set; }
    }

    public class MessageProcessorProvier : ITokenProvider
    {
        public IAsyncUserToken NewAsyncUserToken()
        {
            return new QueuedMessageProcessor();
        }
    }
}