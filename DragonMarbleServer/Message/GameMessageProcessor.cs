using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dragon;
using Dragon.Message;
using log4net;

namespace DragonMarble.Message
{
    public class GameMessageProcessor : IMessageProcessor<IDragonMarbleGameMessage>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (GameMessageProcessor));

        private readonly ManualResetEventSlim _receiveMessageWaitHandler = new ManualResetEventSlim(false);

        private readonly CircularQueue<IDragonMarbleGameMessage> _receivedMessages =
            new CircularQueue<IDragonMarbleGameMessage>();

        public INetworkManager NetworkManager { get; set; }
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadArgs { get; set; }
        public SocketAsyncEventArgs WriteArgs { get; set; }

        private int _resendInterval;
        private int _timeout = Timeout.Infinite;
        private IDragonMarbleGameMessage _timerMessage;

        public IEnumerable<IDragonMarbleGameMessage> ReceivedMessages
        {
            set
            {
                foreach (IDragonMarbleGameMessage gameMessage in value)
                {
                    ReceivedMessage = gameMessage;
                }
            }
        }

        public bool AbleToSend { get; set; }

        /// <summary>
        ///     When received message is none, wait until message received.
        /// </summary>
        public virtual IDragonMarbleGameMessage ReceivedMessage
        {
            get
            {
                if (_receivedMessages.Count < 1)
                {
                    _receiveMessageWaitHandler.Reset();
                    Logger.Debug("no message in queue. wait for message");
                }
                Logger.DebugFormat("Start waiting. {0}, {1}", _receivedMessages.Count, _timeout);

                _receiveMessageWaitHandler.Wait(_timeout);

                Logger.Debug("Deque.");
                return _receivedMessages.Dequeue();
            }
            set
            {
                _receivedMessages.Enqueue(value);
                _receiveMessageWaitHandler.Set();
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("received {0}, It's 1 of {1} queue messages.", value.MessageType,
                        _receivedMessages.Count);
                }
            }
        }

        public IDragonMarbleGameMessage SendingMessage
        {
            set
            {
                while (AbleToSend && _resendInterval < 10000)
                {
                    _resendInterval = _resendInterval << 1;

                    Task.WaitAll(Task.Delay(_resendInterval));

                    AbleToSend = false;
                    WriteArgs.SetBuffer(value.ToByteArray(), 0, value.Length);
                    NetworkManager.SendBytes(Socket, WriteArgs);
                }
            }
            get { throw new NotImplementedException(); }
        }
        
        public void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {
            if (bytesTransferred < sizeof (Int16))
                return;

            short messageLength = BitConverter.ToInt16(buffer, offset);
            if (messageLength == bytesTransferred)
            {
                Convert(buffer, offset, messageLength);
            }
            else if (messageLength > bytesTransferred)
            {
                //TODO
                throw new NotImplementedException("bytes transferred is smaller than message length");
            }
            else if (messageLength < bytesTransferred)
            {
                while (bytesTransferred >= messageLength)
                {
                    Convert(buffer, offset, messageLength);
                    bytesTransferred -= messageLength;
                    offset += messageLength;
                }

                
            }
        }

        private void Convert(byte[] buffer, int offset, int bytesTransferred)
        {
            var bytes = new byte[bytesTransferred];
            Buffer.BlockCopy(buffer, offset, bytes, 0, bytesTransferred);

            ReceivedMessage = GameMessageFactory.GetGameMessage(bytes);
        }
    }
}