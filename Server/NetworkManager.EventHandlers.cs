using System;
using System.Net.Sockets;

namespace Dragon.Server
{
    public partial class NetworkManager
    {
        public ITokenProvider TokenProvider { get; set; }

        public event EventHandler<SocketAsyncEventArgs> OnAfterAccept;
        public event EventHandler<SocketAsyncEventArgs> OnAfterReceive;
        public event EventHandler<SocketAsyncEventArgs> OnAfterSend;
        public event EventHandler<SocketAsyncEventArgs> OnAfterDisconnect;
        
        private void InitializeEventHandler()
        {
            //#1 initialize event handlers
            OnAfterAccept += DefaultAfterAccept;
            
            OnAfterReceive += DefaultAfterReceive;
            
            OnAfterSend += DefaultAfterSend;
            
            OnAfterDisconnect = DefaultAfterDisconnect;

            _state = ManagerState.InitializedEventHandler;
        }

        private void DefaultAfterDisconnect(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DefaultAfterSend(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DefaultAfterReceive(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        // This method is the callback method associated with Socket.AcceptAsync  
        // operations and is invoked when an accept operation is complete 
        // return accept event arg to pool
        // 
        private void DefaultAfterAccept(object sender, SocketAsyncEventArgs e)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Accepted. {0}", sender.GetType());
            }

            //set accepted socket in token
            IAsyncUserToken token = TokenProvider.NewAsyncUserToken();
            token.Socket = e.AcceptSocket;

            //set read write event args
            SocketAsyncEventArgs readArgs = _readPool.Pop();
            readArgs.UserToken = token;

            SocketAsyncEventArgs writeArgs = _writePool.Pop();
            writeArgs.UserToken = token;

            //socket must be cleared since the context object is being reused
            e.AcceptSocket = null;

            _acceptPool.Push(e);

            // Accept the next connection request
            StartAccept();
        }
    }
}