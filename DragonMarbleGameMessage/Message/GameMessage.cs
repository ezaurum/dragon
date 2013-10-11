using System;
using System.Linq;
using System.Net.Mime;
using Dragon.Interfaces;

namespace DragonMarble.Message
{
    public enum GameMessageFlowType
    {
        C2S, S2C
    }

    public class GameMessage : IGameMessage
    {
        public IGameMessageHeader Header { get; set; }
        public IGameMessageBody Body { get; set; }

        public byte[] ToByteArray()
        {
            byte[] result = Body.ToByteArray();
            return Header.ToByteArray(result);
        }

        public GameMessageType MessageType
        {
            get
            {
                return ((GameMessageBody) Body).MessageType;
            }
        }

        public IGameMessageContent Content
        {
            get
            {
                return ((GameMessageBody) Body).Content;
            }
            set
            {
                ((GameMessageBody)Body).Content = value;
            }
        }

        public static GameMessage FromByteArray(byte[] m, GameMessageFlowType flowType)
        {
            GameMessageType gameMessageType = (GameMessageType)BitConverter.ToInt32(m, GameMessageHeader.HeaderLength);

            GameMessage initGameMessage = new GameMessage()
            {
                Header = new GameMessageHeader()
                {
                    MessageLength = (short) m.Length,
                    From = new Guid(m.Skip(GameMessageHeader.FirstGuidIndex).Take(16).ToArray()),
                    To = new Guid(m.Skip(GameMessageHeader.SecondGuidIndex).Take(16).ToArray())
                },
                Body = new GameMessageBody()
                {
                    MessageType = gameMessageType
                }
            };

            if (flowType == GameMessageFlowType.C2S)
            {
                initGameMessage.Content = MakeC2SContent(m, gameMessageType);
            }
            else
            {
                initGameMessage.Content = MakeS2CContent(m, gameMessageType);
            }

            return initGameMessage;
        }

        private static IGameMessageContent MakeS2CContent(byte[] bytes, GameMessageType gameMessageType)
        {
            switch (gameMessageType)
            {
                case GameMessageType.Roll:
                    return new RollMoveDiceResultContent(bytes);
                case GameMessageType.InitilizeBoard:
                    return new InitializeContent();
                default:
                    return null;
            }
        }

        private static IGameMessageContent MakeC2SContent(byte[] m, GameMessageType gameMessageType)
        {
            switch (gameMessageType)
            {
                case GameMessageType.Roll:
                    return new RollMoveDiceContent(m);
                case GameMessageType.InitilizeBoard:
                    return new InitializeContent();
                default:
                    return null;
            }
        }
    }

    public class GameMessageHeader : IGameMessageHeader
    {
        public const Int16 LengthMarkerLength = sizeof(Int16);
        public const Int16 FirstGuidIndex = LengthMarkerLength;
        public const Int16 SecondGuidIndex = LengthMarkerLength + 16;
        public Guid From { get; set; }
        public Guid To { get; set; }
        public const Int16 HeaderLength = SecondGuidIndex + 16;


        public byte[] ToByteArray(byte[] bytes)
        {
            MessageLength = (short) bytes.Length;
            BitConverter.GetBytes(MessageLength).CopyTo(bytes, 0);
            From.ToByteArray().CopyTo(bytes, LengthMarkerLength);
            To.ToByteArray().CopyTo(bytes, SecondGuidIndex);
            return bytes;
        }

        public short MessageLength { get; set; }

        public byte[] ToByteArray()
        {
            return ToByteArray(new byte[HeaderLength]);
        }
    }

    public class GameMessageBody : IGameMessageBody
    {
        public GameMessageType MessageType { get; set; }
        public IGameMessageContent Content { get; set; }

        private const int MessageTypeSize = sizeof(Int32);
        
        public byte[] ToByteArray()
        {
            byte[] contents = Content.ToByteArray();
            byte[] bytes = new byte[contents.Length + GameMessageHeader.HeaderLength + MessageTypeSize];
            
            BitConverter.GetBytes((int)MessageType).CopyTo(bytes, GameMessageHeader.HeaderLength);
            contents.CopyTo(bytes, GameMessageHeader.HeaderLength + MessageTypeSize);
            return bytes;
        }
    }

    public interface IGameMessageContent
    {
        byte[] ToByteArray();
        void FromByteArray(byte[] bytes, int index =  GameMessageHeader.HeaderLength + 4);
    }

    
}