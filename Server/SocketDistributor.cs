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
    public class SocketDistributor<T>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SocketDistributor<T>));
        
        private Socket _listenSocket; 

        private Semaphore _maxConnectionLimit; 

        public SocketDistributor()
        {
            MaximumConnection = int.MaxValue;
        }

        public event EventHandler<SocketAsyncEventArgs> Accepted;

        public int MaximumConnection { private get; set; }

        public IPEndPoint IpEndpoint { private get; set; }
        private SocketAsyncEventArgsPool _acceptPool;
        public Func<IMessageConverter<T, T>> MessageFactoryProvide { get; set; }
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
            catch (SocketException)
            {
                Logger.FatalFormat("Port {0} is Binded already. Abort Starting.",IpEndpoint.Port);
                return;
            }
            
            // start the server with a listen backlog
            _listenSocket.Listen(Backlog);

            // post accepts on the listening socket
            WaitForAccept();
        }

        private void Init()
        {
            //check properties
            if (null == MessageFactoryProvide)
                throw new InvalidOperationException("Message Factory Provide is null.");

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
                Logger.FatalFormat("Backlog must be greater than 0. Current value is {0},", Backlog);
                return;
            }

            if (MaximumConnection < 2)
            {
                Logger.FatalFormat("MaximumConnection must be greater than 1. Current value is {0},", MaximumConnection);
                return;
            }

            //use semaphore for connection limit
            _maxConnectionLimit = new Semaphore(MaximumConnection, MaximumConnection);

            if (null == IpEndpoint)
            {
                if (ListeningPortNumber < 1)
                {
                    Logger.DebugFormat("ListeningPortNumber {0} is not acceptable. Set to default. {1}",
                        ListeningPortNumber, EndPointStorage.DefaultAcceptable.Port);
                    ListeningPortNumber = (ushort) EndPointStorage.DefaultAcceptable.Port;
                }

                if (null == AcceptableIpAddress)
                {
                    Logger.DebugFormat("AcceptableIpAddress {0} is not acceptable. Set to default. {1}",
                        AcceptableIpAddress, EndPointStorage.DefaultAcceptable.Address);
                    AcceptableIpAddress = EndPointStorage.DefaultAcceptable.Address;
                }

                IpEndpoint = new IPEndPoint(AcceptableIpAddress, ListeningPortNumber);
            }

            if (null == _listenSocket)
            {
                // create the socket which listens for incoming connections
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            } 
        }

        /// <summary>
        /// Distribute Dragon Socket as UserToken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DistributeDragonSocket(object sender, SocketAsyncEventArgs e)
        {
            //TODO something... pool
            var dragonSocket = new ServerDragonSocket<T>(e.AcceptSocket, MessageFactoryProvide());
            dragonSocket.Disconnected += SubtractCurrentConnection;
            e.UserToken = dragonSocket;
        }

        private void ReturnToPool(object sender, SocketAsyncEventArgs e)
        {
            //set accept socket null for reuse
            e.AcceptSocket = null;
            
            //return used event args
            _acceptPool.Push(e);
            
            Logger.DebugFormat("SocketError:{0}",e.SocketError);

            WaitForAccept();
        }

        // Begins an operation to accept a connection request from the client  
        private void WaitForAccept()
        {
            //stops when excess max connection
            _maxConnectionLimit.WaitOne();

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
        private void SubtractCurrentConnection(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
#if DEBUG
            int release = 
#endif
                _maxConnectionLimit.Release();

#if DEBUG
            Logger.DebugFormat("disconnected. current connection : {0}", release-1);
#endif
        }
    }
}