using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dragon;
using Dragon.Message;
using DragonMarble.Message;


namespace DragonMarble
{
    public partial class Raja : IMessageProcessor<IDragonMarbleGameMessage>
    {
        private readonly EventWaitHandle _receiveMessageWaitHandler = new ManualResetEvent(false);
        
        private readonly CircularQueue<IDragonMarbleGameMessage> _receivedMessages = new CircularQueue<IDragonMarbleGameMessage>();

        private IDragonMarbleGameMessage _timerMessage;
        private int _timeout = Timeout.Infinite ;
        private int _resendInterval = 0;

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

        /// <summary>
        /// When received message is none, wait until message received.
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

                _receiveMessageWaitHandler.WaitOne(_timeout);
                
                
                    if (null != _timerMessage)
                    {
                        Unit.ControlMode = StageUnitInfo.ControlModeType.AI_0;

                        Logger.DebugFormat("ai");

                        switch (_timerMessage.MessageType)
                        {
                            case GameMessageType.ActivateTurn:
                                ReceivedMessage = new RollMoveDiceGameMessage
                                {
                                    Actor = Unit.Id,
                                    Pressed = RandomUtil.Next(0f, 1f)
                                };
                                Logger.DebugFormat("ai set");
                                break;
                            case GameMessageType.RollMoveDiceResult:
                                break;
                        }     
                        Logger.DebugFormat("ai set");
                    }
                

                Logger.Debug("Deque.");
                return _receivedMessages.Dequeue();
            }
            set
            {
                ResetTimer();
                switch (value.MessageType)
                {
                    case GameMessageType.ReadyState :
                        ReadyStateGameMessage readyStateGameMessage = (ReadyStateGameMessage)value;
                        if (Unit.Id == readyStateGameMessage.Actor)
                        {
                            Unit.IsReady = readyStateGameMessage.Ready;
                            Unit.StageManager.ReadyNotify(new ReadyStateGameMessage
                            {
                                Actor = Unit.Id,
                                Ready = Unit.IsReady
                            });
                        }
                        break;
                    case GameMessageType.StartGame:
                        if (Unit.IsRoomOwner)
                        {
                            ((GameMaster)Unit.StageManager).StartGame(Unit.Id);
                        }
                        break;
                    case GameMessageType.OrderCardSelect:
                        Unit.SelectOrderCard(value);
                        Logger.DebugFormat("received {0}, real time.", value.MessageType);
                        break;
                    case GameMessageType.ActionResultCopy:
                        Logger.DebugFormat("received {0}, real time.", value.MessageType);
                        Unit.IsActionResultCopySended = true;
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

        private void ResetTimer()
        {
            _timeout = Timeout.Infinite;
            _timerMessage = null;
        }

        public IDragonMarbleGameMessage SendingMessage
        {
            set
            {
                SendMessage(value);
                SetTimer(value);
            }
            get { throw new NotImplementedException(); }
        }

        private void SetTimer(IDragonMarbleGameMessage value)
        {
            switch (value.MessageType)
            {
                case GameMessageType.BuyLandRequest:
                    BuyLandRequestGameMessage mBuyLandRequest = (BuyLandRequestGameMessage)value;
                    if ( mBuyLandRequest.Actor != Unit.Id ) return;
                    _timerMessage = value;
                    _timeout = mBuyLandRequest.ResponseLimit;
                    break;

                case GameMessageType.ActivateTurn:
                    ActivateTurnGameMessage mActivateTurn = (ActivateTurnGameMessage)value;
                    if (mActivateTurn.TurnOwner != Unit.Id) return;
                    _timerMessage = value;
                    _timeout = mActivateTurn.ResponseLimit;
                    break;
            }
        }

        public void ResetMessages()
        {
            throw new NotImplementedException();
        }

        public bool AbleToSend { get; set; }

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
            while ( AbleToSend && _resendInterval < 10000)
            {
                _resendInterval = _resendInterval << 1;
                
                Task.WaitAll(Task.Delay(_resendInterval));

                AbleToSend = false;
                WriteArgs.SetBuffer(m.ToByteArray(), 0, m.Length);
                NetworkManager.SendBytes(Socket, WriteArgs);
            }
            
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