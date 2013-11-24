using System;
using System.Net.Sockets;

namespace Dragon.Server
{
    public partial class NetworkManager
    {
        //public IRajaProvider RajaProvider { get; set; }

        public void SendBytes(Socket socket, SocketAsyncEventArgs e)
        {
            Logger.DebugFormat("SocketError:{0}", e.SocketError);
            if (e.SocketError != SocketError.Success) return;
            if (!socket.SendAsync(e))
            {
                OnAfterSend(socket, e);
            }
        }

        public event EventHandler<SocketAsyncEventArgs> OnAfterAccept;
        public event EventHandler<SocketAsyncEventArgs> OnAfterReceive;
        public event EventHandler<SocketAsyncEventArgs> OnAfterSend;
        public event EventHandler<SocketAsyncEventArgs> OnAfterIO;
        public event EventHandler<SocketAsyncEventArgs> OnAfterDisconnect;
        
        private void InitializeEventHandler()
        {
            //#1 initialize event handlers
            OnAfterAccept += ReturnToPoolAfterAccept;
            
            OnAfterReceive += DefaultAfterReceive;
            
            OnAfterSend += DefaultAfterSend;
            
            OnAfterDisconnect = DefaultAfterDisconnect;

            OnAfterIO += DefaultAfterIO;

            OnAfterAccept += OnAfterIO;

            OnAfterReceive += OnAfterIO;

            OnAfterSend += OnAfterIO;

            _state = ManagerState.InitializedEventHandler;
        }

        private void DefaultConnectionAliveCheck(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.ConnectionReset)
            {
                OnAfterDisconnect(null, e);
            }
        }

        private void DefaultAfterDisconnect(object sender, SocketAsyncEventArgs e)
        {
            CleanUpToken(e);
        }

        private void CleanUpToken(SocketAsyncEventArgs e)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Lost connection : {0}", e.SocketError);
            }
            
            /*IRaja token = (IRaja) e.UserToken;

            if (null == token)
                return;
            if (token.IsDisposed)
                return;

            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Clean up token : {0}", token);
            }

            _writePool.Push(token.WriteArgs);
            _readPool.Push(token.ReadArgs);
            token.Dispose();*/
        }

        //send doesn't need continuasly run
        private void DefaultAfterSend(object sender, SocketAsyncEventArgs e)
        {
            Logger.DebugFormat("Sended.");
          /*  IRaja userToken = (IRaja) e.UserToken;
            userToken.AbleToSend = true;*/
        }

        private void DefaultAfterReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("received {0} bytes",e.BytesTransferred);
                }
             /*   IRaja token = (IRaja) e.UserToken;
                token.ReceiveBytes(e.Buffer, e.Offset, e.BytesTransferred);*/
            }
            ReadAsyncRecursive((Socket) sender, e);
        }

        private void DefaultAfterIO(object sender, SocketAsyncEventArgs e)
        {
            DefaultConnectionAliveCheck(e);
        }

        
         
        private void InitToken(object sender, SocketAsyncEventArgs e)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Accepted. {0}", sender.GetType());
            }

            //set accepted socket in token
           /* IRaja token = RajaProvider.NewInstance();
            token.Socket = e.AcceptSocket;
            token.NetworkManager = this;

            e.UserToken = token;*/

            //set read write event args
            SocketAsyncEventArgs readArgs = _readPool.Pop();
           /* readArgs.UserToken = token;
            token.ReadArgs = readArgs;

            SocketAsyncEventArgs writeArgs = _writePool.Pop();
            writeArgs.UserToken = token;
            token.WriteArgs = writeArgs;

            //start read write
            ReadAsyncRecursive(token.Socket, readArgs);*/
        }

        // This method is the callback method associated with Socket.AcceptAsync  
        // operations and is invoked when an accept operation is complete 
        // return accept event arg to pool
        private void ReturnToPoolAfterAccept(object sender, SocketAsyncEventArgs e)
        {
        //socket must be cleared since the context object is being reused
            e.AcceptSocket = null;
            e.UserToken = null;

            _acceptPool.Push(e);

            // Accept the next connection request
            StartAccept();
        }

        private void ReadAsyncRecursive(Socket socket, SocketAsyncEventArgs readArgs)
        {
            if (readArgs.SocketError != SocketError.Success) return;
            if (!socket.ReceiveAsync(readArgs))
            {
                OnAfterReceive(socket, readArgs);
            }
        }
    }
}