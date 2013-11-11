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
            if (bytesTransferred < sizeof(Int16))
            {
                CopyToBuffer(buffer, offset, bytesTransferred);
                return;
            }

            while (bytesTransferred > 0)
            {
                short messageLength = BitConverter.ToInt16(buffer, offset);
                if (1 > messageLength) throw new ArgumentOutOfRangeException("messageLength");
                
                if (messageLength > bytesTransferred)
                {
                    CopyToBuffer(buffer, offset, bytesTransferred);
                    bytesTransferred -= messageLength;
                    offset += messageLength;
                }
                else
                {
                    Convert(buffer, offset, messageLength);
                    bytesTransferred -= messageLength;
                    offset += messageLength;
                }
            }
        }

        private void CopyToBuffer(byte[] buffer, int offset, int bytesTransferred)
        {
            if (bytesTransferred > WriteAbleLength)
            {
                Buffer.BlockCopy(buffer, offset, _buffer, _writeOffset, WriteAbleLength);
                bytesTransferred -= WriteAbleLength;
                offset += WriteAbleLength;
            }

            Buffer.BlockCopy(buffer, offset, _buffer, _writeOffset, bytesTransferred);
            _writeOffset += bytesTransferred;
        }

        private int WriteAbleLength
        {
            get { return _buffer.Length - _writeOffset; }
        }

        [Test]
        public void TestReceiveBytes()
        {
            GameMessageProcessor p = new GameMessageProcessor();

            //case 1 message
            ActivateTurnGameMessage atgm 
                = (ActivateTurnGameMessage) 
                GameMessageFactory.GetGameMessage(GameMessageType.ActivateTurn);
            atgm.ResponseLimit = 10000;
            
            p.ReceiveBytes(atgm.ToByteArray(), 0, atgm.Length);

            Assert.NotNull(p.ReceivedMessage);
            int a = p._receivedMessages.Count;
            Assert.True(a < 1);
        }

        [Test]
        public void TestReceiveBytes0()
        {
            GameMessageProcessor p = new GameMessageProcessor();

            //case 2 messages
            ActivateTurnGameMessage atgm
                = (ActivateTurnGameMessage)
                GameMessageFactory.GetGameMessage(GameMessageType.ActivateTurn);
            atgm.ResponseLimit = 10000;
            
            byte[] buffer1 = new byte[atgm.Length * 2];
            Buffer.BlockCopy(atgm.ToByteArray(), 0, buffer1, 0, atgm.Length);
            Buffer.BlockCopy(atgm.ToByteArray(), 0, buffer1, atgm.Length, atgm.Length);

            p.ReceiveBytes(buffer1, 0, buffer1.Length);

            int count = p._receivedMessages.Count;
            Assert.True(count ==2);
            Assert.NotNull(p.ReceivedMessage);
            Assert.NotNull(p.ReceivedMessage);
            
        }

        [Test]
        public void TestReceiveBytes1()
        {
            GameMessageProcessor p = new GameMessageProcessor();

            //case 1.5 messages
            ActivateTurnGameMessage atgm
                = (ActivateTurnGameMessage)
                GameMessageFactory.GetGameMessage(GameMessageType.ActivateTurn);
            atgm.ResponseLimit = 10000;

            byte[] buffer2 = new byte[atgm.Length + 13];
            Buffer.BlockCopy(atgm.ToByteArray(), 0, buffer2, 0, atgm.Length);
            Buffer.BlockCopy(atgm.ToByteArray(), 0, buffer2, atgm.Length, 13);

            p.ReceiveBytes(buffer2, 0, buffer2.Length);
            
            Assert.True(p._receivedMessages.Count == 1);
            Assert.NotNull(p.ReceivedMessage);
            
        }

        [Test]
        public void TestReceiveBytes2()
        {
            GameMessageProcessor p = new GameMessageProcessor();

            //case 2 messages 1.5 + 0.5
            ActivateTurnGameMessage atgm
                = (ActivateTurnGameMessage)
                GameMessageFactory.GetGameMessage(GameMessageType.ActivateTurn);
            atgm.ResponseLimit = 10000;

            byte[] buffer1 = new byte[atgm.Length * 2];
            Buffer.BlockCopy(atgm.ToByteArray(), 0, buffer1, 0, atgm.Length);
            Buffer.BlockCopy(atgm.ToByteArray(), 0, buffer1, atgm.Length, atgm.Length);

            
            p.ReceiveBytes(buffer1, 0, atgm.Length + 13);
            Assert.True(p._receivedMessages.Count == 1);

            p.ReceiveBytes(buffer1, atgm.Length + 13, atgm.Length - 13);

            Assert.True(p._receivedMessages.Count == 2);
            Assert.NotNull(p.ReceivedMessage);
            Assert.NotNull(p.ReceivedMessage);
            Assert.True(p._receivedMessages.Count < 1);
        }

        private void Convert(byte[] buffer, int offset, int bytesTransferred)
        {
            var bytes = new byte[bytesTransferred];
            Buffer.BlockCopy(buffer, offset, bytes, 0, bytesTransferred);

            ReceivedMessage = GameMessageFactory.GetGameMessage(bytes);
        }
    }
}