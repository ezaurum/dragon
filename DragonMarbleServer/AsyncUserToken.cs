using System;
using System.Net.Sockets;
using Dragon;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public class AsyncUserToken : IAsyncUserToken, IMessageProcessor<IDragonMarbleGameMessage>
    {
        public AsyncUserToken()
        {
            Unit = new StageUnitInfo {MessageProcessor = this};
        }
        public INetworkManager NetworkManager { get; set; }
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadArgs { get; set; }
        public SocketAsyncEventArgs WriteArgs { get; set; }
        public bool IsDisposed { get; set; }

        public StageUnitInfo Unit { get; set; }
        
        public void ReceiveBytes(byte[] buffer, int offset, int bytesTransferred)
        {
            short messageLength = BitConverter.ToInt16(buffer, offset);
            if (messageLength == bytesTransferred)
            {
                byte[] bytes = new byte[bytesTransferred];
                Buffer.BlockCopy(buffer, offset, bytes, 0, bytesTransferred);
                
                //TODO
                Console.WriteLine(GameMessageFactory.GetGameMessage(bytes).MessageType);

                WriteArgs.SetBuffer(bytes, 0, bytesTransferred);
                NetworkManager.SendBytes(Socket, WriteArgs);

            } else if (messageLength > bytesTransferred)
            {
                
            } else if (messageLength < bytesTransferred)
            {
                
            }
            
        }

        public byte[] SendingMessageByteArray()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Socket = null;
            ReadArgs.UserToken = null;
            ReadArgs = null;
            WriteArgs.UserToken = null;
            WriteArgs = null;
            IsDisposed = true;
        }

        public IDragonMarbleGameMessage ReceivedMessage { get; set; }
        public IGameMessage SendingMessage { get; set; }
        public void ResetMessages()
        {
            throw new NotImplementedException();
        }
    }
}