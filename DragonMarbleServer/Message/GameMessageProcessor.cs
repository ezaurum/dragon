using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dragon;
using Dragon.Interfaces;
using Dragon.Message;
using log4net;
using NUnit.Framework;

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
        private int _writeOffset;
        private byte[] _buffer;
        private int _readOffest;

        public GameMessageProcessor()
        {
            _buffer = new byte[1024];
        }

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
                if (_receivedMessages.IsEmpty )
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
            {
                CopyToBuffer(buffer, offset, bytesTransferred);
            }

            if (_writeOffset > sizeof (Int16))
            {
                short length = BitConverter.ToInt16(_buffer, 0);

                int i = _readOffest > _writeOffset ? _readOffest - _writeOffset : _writeOffset - _readOffest;

                if (  i < length)
                {
                    return;
                }
            }

            short messageLength = -1;
            while (bytesTransferred >= messageLength)
            {
                messageLength = BitConverter.ToInt16(buffer, offset);
                Convert(buffer, offset, messageLength);
                bytesTransferred -= messageLength;
                offset += messageLength;
            }

            if (bytesTransferred > 0)
            {
                //TODO byte low
                throw new NotImplementedException("bytes transferred is smaller than message length");
            }
                
            
        }

        private void CopyToBuffer(byte[] buffer, int offset, int bytesTransferred)
        {
            Buffer.BlockCopy(buffer, offset, _buffer, _writeOffset, bytesTransferred);
            _writeOffset += bytesTransferred;
        }

        [Test]
        public void TestReceiveBytes()
        {
            GameMessageProcessor p = new GameMessageProcessor();

            ActivateTurnGameMessage atgm 
                = (ActivateTurnGameMessage) 
                GameMessageFactory.GetGameMessage(GameMessageType.ActivateTurn);
            atgm.ResponseLimit = 10000;
            
            p.ReceiveBytes(atgm.ToByteArray(), 0, atgm.Length);

            Assert.NotNull(p.ReceivedMessage);
            
        }

        private void Convert(byte[] buffer, int offset, int bytesTransferred)
        {
            var bytes = new byte[bytesTransferred];
            Buffer.BlockCopy(buffer, offset, bytes, 0, bytesTransferred);

            ReceivedMessage = GameMessageFactory.GetGameMessage(bytes);
        }
    }
}