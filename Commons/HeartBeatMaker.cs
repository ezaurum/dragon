using System;
using System.Net.Sockets;
using System.Timers;

namespace Dragon
{
    /// <summary>
    /// Heart beat maker
    /// </summary>
    public class HeartBeatMaker<T>
    { 
        private readonly Timer _heartbeatTimer;
        private readonly IMessageSender<T> _sender;
        private readonly T _message;

        public event Action<T> UpdateMessage;

        public HeartBeatMaker(IMessageSender<T> socket, T message, int interval = 60000)
        {
            _sender = socket;
            _message = message; 

            _heartbeatTimer = new Timer { Interval = interval };
            _heartbeatTimer.Elapsed += Beat;
        }

        /// <summary>
        /// Start Beating
        /// </summary>
        public void Start()
        {
            if (null == UpdateMessage)
            {
                throw new InvalidOperationException("Update message is null.");
            }

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
            _sender.Send(_message); 
        }

        public void Stop(object sender, SocketAsyncEventArgs e)
        {
            Stop();
        }
    } 
}