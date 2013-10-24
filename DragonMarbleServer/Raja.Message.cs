using System;
using Dragon;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class Raja : IMessageProcessor<IDragonMarbleGameMessage>
    {
        public IDragonMarbleGameMessage ReceivedMessage { get; set; }
        public IGameMessage SendingMessage { get; set; }

        public void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {
            short messageLength = BitConverter.ToInt16(buffer, offset);
            if (messageLength == bytesTransferred)
            {
                var bytes = new byte[bytesTransferred];
                Buffer.BlockCopy(buffer, offset, bytes, 0, bytesTransferred);

                //TODO
                //Console.WriteLine(GameMessageFactory.GetGameMessage(bytes).MessageType);

                WriteArgs.SetBuffer(bytes, 0, bytesTransferred);
                NetworkManager.SendBytes(Socket, WriteArgs);
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