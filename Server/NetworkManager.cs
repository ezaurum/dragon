using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;
using System.Threading;

namespace Dragon.Server
{
    public class NetworkManager
    {
        private event EventHandler<SocketAsyncEventArgs> OnAfterIO;
        private event EventHandler<SocketAsyncEventArgs> OnAfterAccept;
        private event EventHandler<SocketAsyncEventArgs> OnAfterReceiveMessage;
        private event EventHandler<SocketAsyncEventArgs> OnAfterSendMessage;
        private event EventHandler<SocketAsyncEventArgs> OnAfterDisconnect;

        private const int OpsToPreAlloc = 2; // read, write (don't alloc buffer space for accepts)
        private static readonly ILog Logger = LogManager.GetLogger(typeof (MultiClientServer<>));
        private readonly int _backlog;
        // the maximum number of connections the sample is designed to handle simultaneously  

        private readonly BufferManager _bufferManager;
        // represents a large reusable set of buffers for all socket operations 

        private readonly IPEndPoint _ipEndpoint;
        private readonly ITokenProvider _tokenProvider;

        private readonly Socket _listenSocket; // the socket used to listen for incoming connection requests 
        private readonly Semaphore _maxNumberAcceptedClients;
        private readonly int _numConnections;
        // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
        private readonly SocketAsyncEventArgsPool _readWritePool;
        private readonly SocketAsyncEventArgsPool _acceptPool;
        private int _numConnectedSockets; // the total number of clients connected to the server 
        private int _totalBytesRead; // counter of the total # bytes received by the server 

        // Create an uninitialized server instance.   
        // To start the server listening for connection requests 
        // call the Init method followed by Start method  
        // 
        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public NetworkManager(int numConnections, int receiveBufferSize, int backlog, IPEndPoint ipEndpoint)
        {
            _totalBytesRead = 0;
            _numConnectedSockets = 0;
            _numConnections = numConnections;
            _backlog = backlog;
            _ipEndpoint = ipEndpoint;

            // allocate buffers such that the maximum number of sockets can have one outstanding read and  
            //write posted to the socket simultaneously  
            _bufferManager = new BufferManager(receiveBufferSize*numConnections*OpsToPreAlloc,
                receiveBufferSize);

            _readWritePool = new SocketAsyncEventArgsPool(
                numConnections*OpsToPreAlloc, OnAfterIO, _bufferManager);

            _acceptPool = new SocketAsyncEventArgsPool(numConnections, OnAfterAccept);

            _maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

            // create the socket which listens for incoming connections
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            OnAfterAccept += ProcessAccept;
            OnAfterAccept += DefaultAfterAccept;
        }

        public event EventHandler<SocketAsyncEventArgs> OnReceiveBytes;

        // Starts the server such that it is listening for  
        // incoming connection requests.     
        // 
        public void Start()
        {
            _listenSocket.Bind(_ipEndpoint);
            // start the server with a listen backlog
            _listenSocket.Listen(_backlog);

            // post accepts on the listening socket
            StartAccept();
        }

        // Begins an operation to accept a connection request from the client  
        private void StartAccept()
        {
            Logger.Debug("Start Accpet");
            
            _maxNumberAcceptedClients.WaitOne();

            SocketAsyncEventArgs acceptEventArg 
                = _acceptPool.Count > 1 
                ? _acceptPool.Pop() : _acceptPool.CreateNew(OnAfterAccept);

            Logger.Debug("accept async");
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                Logger.Debug("Direct run");
                OnAfterAccept(_listenSocket, acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync  
        // operations and is invoked when an accept operation is complete 
        // return accept event arg to pool
        // 
        private void DefaultAfterAccept(object sender, SocketAsyncEventArgs e)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Accepted. {0}",sender.GetType());
            }

            //socket must be cleared since the context object is being reused
            e.AcceptSocket = null;

            _acceptPool.Push(e);

            // Accept the next connection request
            StartAccept();

            Logger.DebugFormat("WTF?");
        }

        private void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {
            Interlocked.Add(ref _numConnectedSockets, 2);
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Client connection accepted. There are {0} clients connected to the server", _numConnectedSockets/2);
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Process");
            }


            // Get the socket for the accepted client connection and put it into the  
            //ReadEventArg object user token
            SocketAsyncEventArgs readEventArgs = _readWritePool.Pop();
            SocketAsyncEventArgs writeEventArgs = _readWritePool.Pop();
            
            Socket acceptedSocket = e.AcceptSocket;
            /*
            IAsyncUserToken token = _tokenProvider.NewAsyncUserToken();

            readEventArgs.UserToken = token;
            writeEventArgs.UserToken = token;

            token.Socket = acceptedSocket;*/

            //OnAcceptConnection(this, writeEventArgs);

            // As soon as the client is connected, post a receive to the connection 
            bool willRaiseEvent = acceptedSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            // As soon as the client is connected, post a send to the connection 
            bool willRaiseEvent1 = acceptedSocket.SendAsync(writeEventArgs);
            if (!willRaiseEvent1)
            {
                ProcessSend(writeEventArgs);
            }

            
        }

        // This method is called whenever a receive or send operation is completed on a socket  
        // 
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler 
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                case SocketAsyncOperation.ReceiveMessageFrom:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                case SocketAsyncOperation.SendPackets:
                case SocketAsyncOperation.SendTo:
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    CloseClientSocket(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        // This method is invoked when an asynchronous receive operation completes.  
        // If the remote host closed the connection, then the socket is closed.   
        // If data was received then the data is echoed back to the client. 
        // 
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            IAsyncUserToken token = (IAsyncUserToken) e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                //increment the count of the total bytes receive by the server
                Interlocked.Add(ref _totalBytesRead, e.BytesTransferred);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(string.Format("The server has read a total of {0} bytes", _totalBytesRead));
                    Logger.Debug(string.Format("The server has read {0} bytes", e.BytesTransferred));
                    Logger.Debug(string.Format("The server has read {0} ", e.Offset));
                    Logger.Debug(string.Format("The server has Length {0} ", e.Buffer.Length));
                    Logger.Debug(string.Format("The server has Length {0} ", BitConverter.ToInt16(e.Buffer, e.Offset)));
                }
                OnReceiveBytes(this, e);

                bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        // This method is invoked when an asynchronous send operation completes.   
        // The method issues another receive on the socket to read any additional  
        // data sent from the client 
        // 
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                IAsyncUserToken token = (IAsyncUserToken) e.UserToken;
                // read the next block of data send from the client 

                //block until value return
                byte[] bytes = token.SendingMessageByteArray();
                bytes.CopyTo(e.Buffer, e.Offset);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(string.Format("send:{0} bytes", bytes.Length));
                }


                bool willRaiseEvent = token.Socket.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            IAsyncUserToken token = (IAsyncUserToken) e.UserToken;

            // close the socket associated with the client 
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);
                token.Socket.Disconnect(true);

                Logger.Debug(
                    string.Format(
                        "A client has been disconnected from the server. There are {0} clients connected to the server",
                        _numConnectedSockets)
                    );
            }
                // throws if client process has already closed 
            catch (Exception exception)
            {
                Logger.Debug(exception);
                Logger.Debug(_maxNumberAcceptedClients);
            }
            finally
            {
                // decrement the counter keeping track of the total number of clients connected to the server
                Interlocked.Decrement(ref _numConnectedSockets);
                _maxNumberAcceptedClients.Release();
                // Free the SocketAsyncEventArg so they can be reused by another client
                _readWritePool.Push(e);
            }
        }
    }
}