using System;
using System.Collections.Generic;
using System.Linq;
using Dragon.Interfaces;
using Dragon.Server;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public class GamePlayer : AsyncUserToken, IObserver<GameObject>
    {
        private int _messageStartOffset;
        private int _bytesTransferred;
        private Int16 _messageLength;
        private bool _parsing;
        private const int ResetValue = -1;
        
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GamePlayer));

        public StageUnitInfo Info
        {
            get
            {
                return Unit.Info;
            }
        }

        public StageUnit Unit { get; set; }
        public GameMaster GameMaster { get; set; }

        public GamePlayer()
        {
            Unit = new StageUnit(StageUnitInfo.TEAM_COLOR.RED, 1000000, Guid.NewGuid().ToString());
        }

        public void OnNext(GameObject value)
        {
            
        }

        public void OnError(Exception error)
        {
            Logger.Debug("ERROR");
            OnCompleted();
        }

        public void OnCompleted()
        {
            
        }

        public byte[] ToByte(IGameMessage message)
        {
            return message.ToByteArray();
        }

        public IEnumerable<IGameMessage> ToMessages(byte[] buffer, int offset, int bytesTransferred)
        {
            List<IGameMessage> messages = new List<IGameMessage>();

            while (true)
            {
                //start new message
                IGameMessage message = ToMessage(buffer, ref offset, ref bytesTransferred);
                if (null == message) break;
                messages.Add(message);
            }

            return messages;
        }

        private IGameMessage ToMessage(byte[] buffer, ref int offset, ref int bytesTransferred)
        {
            while (true)
            {
                Logger.Debug("Parse");
                if (_parsing)
                {
                    Logger.Debug("during parse");
                    offset = _messageStartOffset;
                    bytesTransferred += _bytesTransferred;
                    continue;
                }

                _parsing = true;
                _messageStartOffset = offset;
                _bytesTransferred = bytesTransferred;

                //message length is not sufficient
                if (bytesTransferred < 2)
                {
                    Logger.Debug("message length not trasffered all");
                    return null;
                }
                _messageLength = BitConverter.ToInt16(buffer, offset);

                //message not transferred all
                if (_messageLength > _bytesTransferred)
                {
                    Logger.Debug("message not trasffered all");
                    return null;
                }

                //make new game message
                IGameMessage result = new GameMessage()
                {
                    Header = new GameMessageHeader()
                    {
                        MessageLength = _messageLength,
                        From = new Guid(buffer.Skip(offset + GameMessageHeader.FirstGuidIndex).Take(16).ToArray()),
                        To = new Guid(buffer.Skip(offset + GameMessageHeader.SecondGuidIndex).Take(16).ToArray()),
                    },
                    Body = new GameMessageBody()
                    {
                    }
                };

                bytesTransferred -= _messageLength;
                offset += _messageLength;

                //reset message offes and etc.
                _messageLength = ResetValue;
                _messageStartOffset = ResetValue;
                _bytesTransferred = ResetValue;
                _parsing = false;

                return result;
            }
        }
    }
}