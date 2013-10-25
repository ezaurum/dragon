using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public event ReceivedMessageEventHandler ReceivedMessageCompleted;

        public delegate void ReceivedMessageEventHandler(object sender, ReceivedMessageEventHandlerArgs args);

        public class ReceivedMessageEventHandlerArgs : EventArgs
        {

        }

        public IDragonMarbleGameMessage ReceivedMessage
        {
            get
            {
                if (_receivedMessages.Count < 1)
                {
                    //TODO is this ok?
                    Task.WaitAll(new Task(() =>
                    {
                        while (_receivedMessages.Count < 1)
                        {
                        }}));
                }

                return _receivedMessages.Dequeue();
            }
            set
            {
                _receivedMessages.Enqueue(value);
                Unit.ReceivedMessage = value;
                Logger.DebugFormat("received message {0}",value.MessageType);
            }
        }

        public IDragonMarbleGameMessage SendingMessage
        {
            get
            {
                if (_sendingMessages.Count < 1)
                {
                    //TODO is this ok?
                    Task.WaitAll(new Task(() =>
                    {
                        while (_receivedMessages.Count < 1)
                        {
                        }
                    }));
                }
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