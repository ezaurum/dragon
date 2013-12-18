using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Dragon.Server;
using log4net;

namespace Dragon
{
    /// <summary>
    ///     Accept Connection and Distribute sockets
    ///     Set AccpetPool needed
    /// </summary>
    public class SocketDistributor<T> :EndPointStorage where T : IMessage
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SocketDistributor<T>));

        private int _currentAcceptedConnections;
        private Socket _listenSocket;
        private DistributorState _state = DistributorState.BeforeInitialized;

        public SocketDistributor()
        {
            MaximumConnection = int.MaxValue;
        }

        public event EventHandler<SocketAsyncEventArgs> Accepted;

        public int MaximumConnection { private get; set; }
        public IPEndPoint IpEndpoint { private get; set; }
        private SocketAsyncEventArgsPool _acceptPool;
        public IMessageFactory<T> MessageFactory { get; set; }
        public int Backlog { private get; set; }
        public UInt16 ListeningPortNumber { get; set; }
        public IPAddress AcceptableIpAddress { get; set; }
        
        public void Start()
        {
            Init();

            try
            {
                _listenSocket.Bind(IpEndpoint);
            }
            catch (SocketException e)
            {
                Logger.FatalFormat("Port {0} is Binded already. Abort Starting.",IpEndpoint.Port);
                return;
            }
            _state = DistributorState.NotAccpetable;
            
            // start the server with a listen backlog
            _listenSocket.Listen(Backlog);

            _state = DistributorState.Acceptable;

            // post accepts on the listening socket
            WaitForAccept();
        }

        private void Init()
        {
            //check states
            if (_state > DistributorState.BeforeInitialized)
            {
                string message = string.Format("Initialized already. Current state is {0}.", _state);
                Logger.Fatal(message);
                throw new InvalidOperationException(message);
            }

            if (_currentAcceptedConnections > 0)
            {
                string message = string.Format("Accepted {0} Connections is exist.",_currentAcceptedConnections);
                Logger.Fatal(message);
                throw new InvalidOperationException(message);
            }
            
            //check properties
            if (null == MessageFactory)
                throw new InvalidOperationException("Message Factory is null.");

            _acceptPool = new SocketAsyncEventArgsPool();
            //first sequence. 
            _acceptPool.Completed += DistributeDragonSocket;

            if (null != Accepted )
                _acceptPool.Completed += Accepted;
            
            //last sequence
            _acceptPool.Completed += ReturnToPool;
            _acceptPool.Prepare(Backlog);

            if (Backlog < 1)
            {
                var message = string.Format("Backlog must be greater than 0. Current value is {0},", Backlog);
                Logger.Fatal(message);
                throw new InvalidOperationException(message);
            }

            if (MaximumConnection < 2)
            {
                throw new InvalidOperationException(
                    string.Format("MaximumConnection must be greater than 1. Current value is {0},", MaximumConnection));
            }

            if (null == IpEndpoint)
            {
                if (ListeningPortNumber < 1)
                {
                    Logger.DebugFormat("ListeningPortNumber {0} is not acceptable. Set to default. {1}",
                        ListeningPortNumber, DefaultAcceptable.Port);
                    ListeningPortNumber = (ushort) DefaultAcceptable.Port;
                }

                if (null == AcceptableIpAddress)
                {
                    Logger.DebugFormat("AcceptableIpAddress {0} is not acceptable. Set to default. {1}",
                        AcceptableIpAddress, DefaultAcceptable.Address);
                    AcceptableIpAddress = DefaultAcceptable.Address;
                }

                IpEndpoint = new IPEndPoint(AcceptableIpAddress, ListeningPortNumber);
            }

            if (null == _listenSocket)
            {
                // create the socket which listens for incoming connections
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            _state = DistributorState.Initialized;
        }

        /// <summary>
        /// Distribute Dragon Socket as UserToken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DistributeDragonSocket(object sender, SocketAsyncEventArgs e)
        {
            //TODO something... pool
            var dragonSocket = new ServerDragonSocket<T>(e.AcceptSocket, MessageFactory);
            e.UserToken = dragonSocket;
        }

        private void ReturnToPool(object sender, SocketAsyncEventArgs e)
        {
            //set accept socket null for reuse
            e.AcceptSocket = null;
            
            //return used event args
            _acceptPool.Push(e);

            //check successfully accept
            if (e.SocketError == SocketError.Success)
            {
                //add connection number
                Interlocked.Increment(ref _currentAcceptedConnections);
                
                //check connection max number
                CheckMaxNumber();
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("SocketError:{0}",e.SocketError);
                Logger.DebugFormat("Current Connection:{0}", _currentAcceptedConnections);
            }

            if (_state < DistributorState.Acceptable)
            {
                string message = string.Format("Not initialized. Current State is {0}", _state);
                Logger.Fatal(message);
                throw new InvalidOperationException(message);
            }

            WaitForAccept();
        }

        private void CheckMaxNumber()
        {
            _state = _currentAcceptedConnections < MaximumConnection
                ? DistributorState.Acceptable
                : DistributorState.ExceedMaximumConnection;
                
            
            if (_state == DistributorState.ExceedMaximumConnection)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Exceed Maximum Connection {0}/{1}", _currentAcceptedConnections,
                        MaximumConnection);
                }
            }
        }

        // Begins an operation to accept a connection request from the client  
        private void WaitForAccept()
        {
            if (_state < DistributorState.Acceptable)
            {
                Logger.DebugFormat("Not Acceptable. Current state : {0}",_state);
                return;
            }

            Logger.Debug("Wait for Accpet");

            SocketAsyncEventArgs socketAsyncEventArgs = _acceptPool.Pop();

            if (!_listenSocket.AcceptAsync(socketAsyncEventArgs))
            {
                ReturnToPool(null, socketAsyncEventArgs);
            }
            
        }

        /// <summary>
        ///     when
        /// </summary>
        public void SubtractCurrentConnection()
        {
            Interlocked.Decrement(ref _currentAcceptedConnections);

            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Current Connection:{0}", _currentAcceptedConnections);
            }

            if (_currentAcceptedConnections < 0)
            {
                throw new InvalidOperationException(string.Format("Current Connection is {0}. ",
                    _currentAcceptedConnections));
            }

            if (_state == DistributorState.ExceedMaximumConnection)
            {
                CheckMaxNumber();
            }
        }

        private enum DistributorState
        {
            BeforeInitialized = 0,
            Initialized = 1,
            NotAccpetable = 2,
            ExceedMaximumConnection = 3,
            Acceptable = 4,
        }
    }
}