using System;
using System.Net;
using System.Net.Sockets;
using log4net;
using System.Threading;

namespace Dragon.Server
{
    
    public class NetworkManager : INetworkManager
    {
        private enum ManagerState
        {
            BeforeInitialized = 0,
            InitializedFields = 100,
            InitializedEventHandler,
            InitializedHelperObjects,
            Running = 1000
        }
        public event EventHandler<SocketAsyncEventArgs> OnAfterIO;
        public event EventHandler<SocketAsyncEventArgs> OnAfterAccept;
        public event EventHandler<SocketAsyncEventArgs> OnAfterReceive;
        public event EventHandler<SocketAsyncEventArgs> OnAfterSend;
        public event EventHandler<SocketAsyncEventArgs> OnAfterDisconnect;

        private const int OpsToPreAlloc = 2; // read, write (don't alloc buffer space for accepts)
        private static readonly ILog Logger = LogManager.GetLogger(typeof (MultiClientServer<>));
        private readonly int _backlog;
        // the maximum number of connections the sample is designed to handle simultaneously  

        private BufferManager _bufferManager;
        // represents a large reusable set of buffers for all socket operations 

        private readonly IPEndPoint _ipEndpoint;

        private Socket _listenSocket; // the socket used to listen for incoming connection requests 
        private Semaphore _maxNumberAcceptedClients;
        private readonly int _numConnections;
        private readonly int _receiveBufferSize;
        // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
        private SocketAsyncEventArgsPool _readPool;
        private SocketAsyncEventArgsPool _writePool;
        private SocketAsyncEventArgsPool _acceptPool;
        private int _numConnectedSockets; // the total number of clients connected to the server 
        private int _totalBytesRead; // counter of the total # bytes received by the server 
        private ManagerState _state;

        // Create an uninitialized server instance.   
        // To start the server listening for connection requests 
        // call the Init method followed by Start method  
        // 
        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public NetworkManager(int numConnections, int receiveBufferSize, int backlog, IPEndPoint ipEndpoint)
        {
            //#0 initialize fields value
            _totalBytesRead = 0;
            _numConnectedSockets = 0;
            _numConnections = numConnections;
            _receiveBufferSize = receiveBufferSize;
            _backlog = backlog;
            _ipEndpoint = ipEndpoint;
            
            _state = ManagerState.InitializedFields;
        }

        public void Init()
        {
            InitializeEventHandler();
            InitializeHelperObject();
        }

        private void InitializeEventHandler()
        {
            //#1 initialize event handlers
            OnAfterAccept += ProcessAccept;
            OnAfterAccept += DefaultAfterAccept;

            OnAfterReceive += DefaultAfterReceive;
            OnAfterReceive += OnAfterIO;

            OnAfterSend += DefaultAfterSend;
            OnAfterSend += OnAfterIO;

            OnAfterDisconnect = DefaultAfterDisconnect;
            
            _state = ManagerState.InitializedEventHandler;
        }

        private void InitializeHelperObject()
        {
            if ( ManagerState.InitializedEventHandler > _state)
            {
                throw new InvalidOperationException("Not initialized.");
            }
            
            //#2 initialize helper objects, 
            // event handlers should be initialized.
            // allocate buffers such that the maximum number of sockets can have one outstanding read and  
            //write posted to the socket simultaneously  
            _bufferManager = new BufferManager(_receiveBufferSize*_numConnections*OpsToPreAlloc,
                _receiveBufferSize);

            _readPool = new SocketAsyncEventArgsPool(
                _numConnections, OnAfterReceive, _bufferManager);

            _writePool = new SocketAsyncEventArgsPool(
                _numConnections, OnAfterSend, _bufferManager);

            _acceptPool = new SocketAsyncEventArgsPool(_numConnections, OnAfterAccept);

            _maxNumberAcceptedClients = new Semaphore(_numConnections, _numConnections);

            // create the socket which listens for incoming connections
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _state = ManagerState.InitializedHelperObjects;
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


        // Starts the server such that it is listening for  
        // incoming connection requests.     
        // 
        public void Start()
        {
            if (ManagerState.InitializedHelperObjects >= _state)
            {
                Init();
            }

            _state = ManagerState.Running;

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
                    ? _acceptPool.Pop()
                    : _acceptPool.CreateNew(OnAfterAccept);

            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                Logger.Debug("Direct run");
                OnAfterAccept.Invoke(_listenSocket, acceptEventArg);
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
                Logger.DebugFormat("Accepted. {0}", sender.GetType());
            }

            Interlocked.Add(ref _numConnectedSockets, 2);

            //socket must be cleared since the context object is being reused
            e.AcceptSocket = null;

            _acceptPool.Push(e);

            // Accept the next connection request
            StartAccept();

            Logger.DebugFormat("WTF Default ?");
        }

        private void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {

            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Client connection accepted. There are {0} clients connected to the server",
                    _numConnectedSockets/2);
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Process");
            }

        }
        

        // This method is called whenever a receive or send operation is completed on a socket  
        // 
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        private void AfterIO(object sender, SocketAsyncEventArgs e)
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
                //OnReceiveBytes(this, e);

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
                //TODO how to return object to pool?
                _readPool.Push(e);
            }
        }
    }
}