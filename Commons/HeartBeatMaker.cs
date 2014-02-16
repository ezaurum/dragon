using System;
using System.Timers;

namespace Dragon
{
    /// <summary>
    /// Heart beat maker
    /// </summary>
    public class HeartBeatMaker<T> where T : IMessage
    { 
        private readonly Timer _heartbeatTimer;
        private readonly IDragonSocket<T> _socket;
        private readonly T _message;

        public event Action<T> UpdateMessage;

        public HeartBeatMaker(IDragonSocket<T> socket, T message, int interval = 1000)
        {
            _socket = socket;
            _message = message; 

            _heartbeatTimer = new Timer { Interval = interval };
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