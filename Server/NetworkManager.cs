using System;
using System.Net;
using System.Net.Sockets;
using log4net;
using System.Threading;

namespace Dragon.Server
{
    public partial class NetworkManager : INetworkManager
    {
        private enum ManagerState
        {
            BeforeInitialized = 0,
            InitializedFields = 100,
            InitializedEventHandler,
            InitializedHelperObjects,
            Running = 1000
        }

        private const int OpsToPreAlloc = 2; // read, write (don't alloc buffer space for accepts)
        private static readonly ILog Logger = LogManager.GetLogger(typeof (NetworkManager));
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
        private ManagerState _state = ManagerState.BeforeInitialized;

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
                _numConnections, OnAfterSend);

            _acceptPool = new SocketAsyncEventArgsPool(_numConnections, OnAfterAccept);

            _maxNumberAcceptedClients = new Semaphore(_numConnections, _numConnections);

            // create the socket which listens for incoming connections
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _state = ManagerState.InitializedHelperObjects;
        }

        // Starts the server such that it is listening for  
        // incoming connection requests.
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
                OnAfterAccept(_listenSocket, acceptEventArg);
            }
        }
    }
}