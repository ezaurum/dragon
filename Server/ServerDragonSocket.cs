using System;
using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    /// Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServerDragonSocket<T> : DragonSocket<T> where T : IMessage
    {
        public ServerDragonSocket(Socket acceptSocket, IMessageFactory<T> factory) : base(factory)
        {
            Socket = acceptSocket;

            if (null != Accepted)
                Accepted(Socket, null);

            _messageConverter.HeartbeatedHeard += OnMessageConverterOnHeartbeatedHeard;
        }

        private void OnMessageConverterOnHeartbeatedHeard()
        {
            _last2Heartbeat = _lastHeartbeat;
            _lastHeartbeat = DateTime.Now;

            if (null != HeartbeatHeard)
                HeartbeatHeard();
        }

        private DateTime _lastHeartbeat;
        private DateTime _last2Heartbeat;
        
        public event EventHandler<SocketAsyncEventArgs> Accepted;
        public event VoidMessageEventHandler HeartbeatHeard;
    }
}