using System;
using Dragon.Interfaces;

namespace DragonMarble.Message
{
    public enum GameMessageFlowType
    {
        C2S, S2C
    }

    public class GameMessage : IGameMessage
    {
        public GameMessage(Guid from, Guid to, Guid messageSource, GameMessageType messageType, IGameMessageContent messageContent)
        {
            Header = new GameMessageHeader
            {
                From = from,
                To = to
            };
            Body = new GameMessageBody
            {
                MessageType = GameMessageType.Inform,
                Content = new InformContent(messageSource, messageType, messageContent)
            };
        }

        public GameMessage()
        {
            
        }

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

            byte[] fromBytes = new byte[16];
            Buffer.BlockCopy(m, GameMessageHeader.FirstGuidIndex, fromBytes, 0,16);

            byte[] toBytes = new byte[16];
            Buffer.BlockCopy(m, GameMessageHeader.SecondGuidIndex, fromBytes, 0, 16);

            GameMessage initGameMessage = new GameMessage()
            {
                Header = new GameMessageHeader()
                {
                    MessageLength = (short) m.Length,
                    From = new Guid(fromBytes),
                    To = new Guid(toBytes)
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
                case GameMessageType.RollMoveDice:
                    return new RollMoveDiceResultContent(bytes);
                case GameMessageType.InitializeGame:
                    return new InitializeContent();
                default:
                    return null;
            }
        }

        private static IGameMessageContent MakeC2SContent(byte[] m, GameMessageType gameMessageType)
        {
            switch (gameMessageType)
            {
                case GameMessageType.RollMoveDice:
                    return new RollMoveDiceContent(m);
                case GameMessageType.InitializeGame:
                    return new InitializeContent();
                default:
                    return null;
            }
        }
    }

    /**
     * inform something action for everyone
     */
    public class InformContent : IGameMessageContent
    {
        public IGameMessageContent Content { get; set; }
        public GameMessageType MessageType { get; set; }
        public Guid MessageSource { get; set; }
        public InformContent(Guid messageSource, GameMessageType messageType, IGameMessageContent messageContent)
        {
            MessageSource = messageSource;
            MessageType = messageType;
            Content = messageContent;
        }

        public byte[] ToByteArray()
        {
            byte[] byteArray = Content.ToByteArray();
            byte[] bytes = new byte[20+byteArray.Length];
            MessageSource.ToByteArray().CopyTo(bytes, 0);
            BitConverter.GetBytes((int) MessageType).CopyTo(bytes, 16);
            Buffer.BlockCopy(byteArray,0,bytes,20, byteArray.Length);
            return bytes;
        }

        public void FromByteArray(byte[] bytes, int index = 38)
        {
            throw new NotImplementedException();
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

    public static class BitConvertUtils
    {
        public static void WriteBytes(Int16 valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(Int16);
        }

        public static void WriteBytes(Int32 valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(Int32);
        }
        public static void WriteBytes(bool valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(bool);
        }

        public static void WriteBytes(char valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(char);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref char value)
        {
            value = BitConverter.ToChar(bytes, index);
            index += sizeof(char);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref bool value)
        {
            value = BitConverter.ToBoolean(bytes, index);
            index += sizeof(bool);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref Int16 value)
        {
            value = BitConverter.ToInt16(bytes, index);
            index += sizeof(Int16);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref Int32 value)
        {
            value = BitConverter.ToInt32(bytes, index);
            index += sizeof(Int32);
        }
    }
}