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
    public class SocketDistributor
    {
        private const int DefaultListeningPortNumber = 10008;
        private static readonly IPAddress DefaultAcceptableIpAddress = IPAddress.Any;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SocketDistributor));

        private int _currentAcceptedConnections;
        private Socket _listenSocket;
        private DistributorState _state = DistributorState.BeforeInitialized;

        public SocketDistributor()
        {
            MaximumConnection = int.MaxValue;
        }

        public event EventHandler<SocketAsyncEventArgs> OnSessionProvide;

        public int MaximumConnection { private get; set; }
        public IPEndPoint IpEndpoint { private get; set; }
        public SocketAsyncEventArgsPool AcceptPool { private get; set; }
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
                Logger.DebugFormat("Port {0} is Binded already. Abort Starting.",IpEndpoint.Port);
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
                throw new InvalidOperationException(string.Format("Initialized already. Current state is {0}.", _state));
            }

            if (_currentAcceptedConnections > 0)
            {
                throw new InvalidOperationException(string.Format("Accepted {0} Connections is exist.",
                    _currentAcceptedConnections));
            }

            //check properties
            if (null == AcceptPool)
            {
                throw new InvalidOperationException("AcceptPool is null.");
            }
            
            if (null != OnSessionProvide )
                AcceptPool.Completed += OnSessionProvide;

            AcceptPool.Completed += ReturnToPool;
            AcceptPool.Prepare(Backlog);

            if (Backlog < 1)
            {
                throw new InvalidOperationException(
                    string.Format("Backlog must be greater than 0. Current value is {0},", Backlog));
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
                        ListeningPortNumber, DefaultListeningPortNumber);
                    ListeningPortNumber = DefaultListeningPortNumber;
                }

                if (null == AcceptableIpAddress)
                {
                    Logger.DebugFormat("AcceptableIpAddress {0} is not acceptable. Set to default. {1}",
                        AcceptableIpAddress, DefaultAcceptableIpAddress);
                    AcceptableIpAddress = DefaultAcceptableIpAddress;
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

        private void ReturnToPool(object sender, SocketAsyncEventArgs e)
        {
            //set accept socket null for reuse
            e.AcceptSocket = null;
            
            //return used event args
            AcceptPool.Push(e);

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
                throw new InvalidOperationException(string.Format("Not initialized. Current State is {0}", _state));
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

            SocketAsyncEventArgs socketAsyncEventArgs = AcceptPool.Pop();

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