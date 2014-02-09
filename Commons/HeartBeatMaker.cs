using System;
using System.Net.Sockets;
using System.Timers;

namespace Dragon
{
    /// <summary>
    /// Heart beat maker
    /// </summary>
    public class HeartBeatMaker
    {
        private readonly SocketAsyncEventArgs _heartbeatEventArgs;
        private readonly Timer _heartbeatTimer;
        private readonly Socket _socket;
        public event EventHandler<SocketAsyncEventArgs> OnSocketError;

        public HeartBeatMaker(Socket socket)
        {
            _socket = socket;

            //set heartbeats
            byte[] heartbeatBuffer = BitConverter.GetBytes((Int16)sizeof(Int16));
            _heartbeatEventArgs = new SocketAsyncEventArgs();
            _heartbeatEventArgs.Completed += OnHeartbeat;
            _heartbeatEventArgs.SetBuffer(heartbeatBuffer, 0, sizeof(Int16));
            _heartbeatTimer = new Timer { Interval = 1000 };
            _heartbeatTimer.Elapsed += Beat;
        }

        /// <summary>
        /// Start Beating
        /// </summary>
        public void Start()
        {
            _heartbeatTimer.Start();
        }

        /// <summary>
        /// End Beat
        /// </summary>
        public void Stop()
        {
            _heartbeatTimer.Stop();
        }

        private void Beat(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _heartbeatEventArgs.SetBuffer(0, sizeof(Int16));
            if (_socket.SendAsync(_heartbeatEventArgs)) return;
            OnHeartbeat(_socket, _heartbeatEventArgs);
        }

        private void OnHeartbeat(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success) return;
            if (_heartbeatTimer.Enabled) _heartbeatTimer.Stop();
            if (null == OnSocketError) return;
            OnSocketError(sender, e);
        }
    }


    /// <summary>
    /// Heart beat maker
    /// </summary>
    public class HeartBeatMaker<T> where T : IMessage
    { 
        private readonly Timer _heartbeatTimer;
        private readonly IDragonSocket<T> _socket;
        private readonly T _message;

        public event Action<T> UpdateMessage;

        public HeartBeatMaker(IDragonSocket<T> socket, T message)
        {
            _socket = socket;
            _message = message; 

            _heartbeatTimer = new Timer { Interval = 1000 };
            _heartbeatTimer.Elapsed += Beat;
        }

        /// <summary>
        /// Start Beating
        /// </summary>
        public void Start()
        {
            _heartbeatTimer.Start();
        }

        /// <summary>
        /// End Beat
        /// </summary>
        public void Stop()
        {
            _heartbeatTimer.Stop();
        }

        private void Beat(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            UpdateMessage(_message);
            _socket.Send(_message); 
        }
    } 
}