﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Dragon.Interfaces;
using log4net;

namespace Dragon.Server
{
    // user token for async process.
    public class QueuedMessageProcessor<T> : IAsyncUserToken, IMessageProcessor<T> where T : IGameMessage
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueuedMessageProcessor<>));
        private readonly Guid _id = Guid.NewGuid();
        public Guid Id { get { return _id; } }
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadArg { get; set; }
        public SocketAsyncEventArgs WriteArg { get; set; }

        private readonly Queue<T> _receivedMessages = new Queue<T>();
        private readonly Queue<T> _sendingMessages = new Queue<T>();

        private readonly EventWaitHandle _receiveMessageWaitHandler = new ManualResetEvent(false);
        private readonly EventWaitHandle _sendMessageWaitHandler = new ManualResetEvent(false);

        public IEnumerable<T> ReceivedMessages
        {
            set
            {
                foreach (T gameMessage in value)
                {
                    ReceivedMessage = gameMessage;
                }
            }
        }

        public virtual  T ReceivedMessage
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
                _sendingMessages.Enqueue((T) value);
                _sendMessageWaitHandler.Set();
            }
        }

        public object Player { get; set; }
    }

    public class MessageProcessorProvier<T> : ITokenProvider where T : IGameMessage
    {
        public IAsyncUserToken NewAsyncUserToken()
        {
            return new QueuedMessageProcessor<T>();
        }
    }
}