using System;
using System.Collections.Generic;
using Dragon;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class Raja : IMessageProcessor<IDragonMarbleGameMessage>
    {
        private readonly Queue<IDragonMarbleGameMessage> _sendingMessages 
            = new Queue<IDragonMarbleGameMessage>();

        private readonly Queue<IDragonMarbleGameMessage> _receivedMessages
            = new Queue<IDragonMarbleGameMessage>();

        public IDragonMarbleGameMessage ReceivedMessage
        {
            get
            {
                return _receivedMessages.Dequeue();
            }
            set
            {
                _receivedMessages.Enqueue(value);
                Unit.ReceivedMessage = value;
            }
        }

        public IDragonMarbleGameMessage SendingMessage
        {
            get
            {
                return _sendingMessages.Dequeue();
            }
            set
            {
                _sendingMessages.Enqueue(value);

                SendMessage(value);
            } 
        }

        private void SendMessage(IDragonMarbleGameMessage m)
        {
            WriteArgs.SetBuffer(m.ToByteArray(), 0, m.Length);
            NetworkManager.SendBytes(Socket, WriteArgs);
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
            }
            else if (messageLength < bytesTransferred)
            {
                //TODO
            }
        }


        public void ResetMessages()
        {
            throw new NotImplementedException();
        }

        public byte[] SendingMessageByteArray()
        {
            throw new NotImplementedException();
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