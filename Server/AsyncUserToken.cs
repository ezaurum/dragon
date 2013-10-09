using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Dragon.Interfaces;
using log4net;

namespace Dragon.Server
{
    // user token for async process.
    public class AsyncUserToken
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AsyncUserToken));
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadArg { get; set; }
        public SocketAsyncEventArgs WriteArg { get; set; }
        public IMessageParser Parser { get; set; }
        public IActionRunner Runner { get; set; }

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

        public IGameMessage ReceivedMessage
        {
            get
            {   
                if (_receivedMessages.Count < 1)
                {
                    _receiveMessageWaitHandler.Reset();
                }
                _receiveMessageWaitHandler.WaitOne();
                return _receivedMessages.Dequeue();
            }
            set
            {
                _receivedMessages.Enqueue(value);
                _receiveMessageWaitHandler.Set();
            }
        }

        public IGameMessage SendingMessage
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
    }

    public interface IActionRunner
    {
        AsyncUserToken NewAsyncUserToken();
    }
}