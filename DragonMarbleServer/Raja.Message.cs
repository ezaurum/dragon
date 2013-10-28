using System;
using System.Collections.Generic;
using System.Threading;
using Dragon;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class Raja : IMessageProcessor<IDragonMarbleGameMessage>
    {
        private readonly EventWaitHandle _receiveMessageWaitHandler = new ManualResetEvent(false);

        private readonly Queue<IDragonMarbleGameMessage> _receivedMessages
            = new Queue<IDragonMarbleGameMessage>();

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

        public virtual IDragonMarbleGameMessage ReceivedMessage
        {
            get
            {
                if (_receivedMessages.Count < 1)
                {
                    _receiveMessageWaitHandler.Reset();
                    Logger.Debug("no message in queue. wait for message");
                }
                Logger.DebugFormat("Start waiting. {0}", _receivedMessages.Count);
                _receiveMessageWaitHandler.WaitOne();
                Logger.Debug("Deque.");
                return _receivedMessages.Dequeue();
            }
            set
            {
                switch (value.MessageType)
                {
                    case GameMessageType.OrderCardSelect:
                        Unit.ReceivedMessage = value;
                        Logger.DebugFormat("received {0}, real time.", value.MessageType);
                        break;
                    default:
                        _receivedMessages.Enqueue(value);
                        _receiveMessageWaitHandler.Set();
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.DebugFormat("received {0}, It's 1 of {1} queue messages.", value.MessageType,
                                _receivedMessages.Count);
                        }
                        break;
                }
            }
        }

        public IDragonMarbleGameMessage SendingMessage
        {
            set
            {
                SendMessage(value);
            }
            get { throw new NotImplementedException(); }
        }


        public void ResetMessages()
        {
            throw new NotImplementedException();
        }

        public void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {
            short messageLength = BitConverter.ToInt16(buffer, offset);
            if (messageLength == bytesTransferred)
            {
                var bytes = new byte[bytesTransferred];
                Buffer.BlockCopy(buffer, offset, bytes, 0, bytesTransferred);

                ReceivedMessage = GameMessageFactory.GetGameMessage(bytes);
            }
            else if (messageLength > bytesTransferred)
            {
                //TODO
                throw new NotImplementedException("bytes transferred is smaller than message length");
            }
            else if (messageLength < bytesTransferred)
            {
                //TODO
                throw new NotImplementedException("bytes transferred is bigger than message length");
            }
        }

        private void SendMessage(IDragonMarbleGameMessage m)
        {
            Logger.DebugFormat("1SendMessage:{0}, {1}bytes", m.MessageType, m.Length);
            WriteArgs.SetBuffer(m.ToByteArray(), 0, m.Length);
            NetworkManager.SendBytes(Socket, WriteArgs);
            Logger.DebugFormat("2SendMessage:{0}, {1}bytes", m.MessageType, m.Length);
        }
    }


    public class RajaProvider : IRajaProvider
    {
        public IRaja NewInstance()
        {
            return new Raja();
        }
    }
}