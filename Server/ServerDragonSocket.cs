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
        private readonly HeartBeatMaker _heartBeatMaker;

        public ServerDragonSocket(Socket acceptSocket, IMessageFactory<T> factory) : base(factory)
        {
            Socket = acceptSocket;
            _heartBeatMaker = new HeartBeatMaker(Socket);

            if (null != Accepted)
            {
                Accepted(Socket, null);
            }
            
            MessageConverter.HeartbeatedHeard += OnMessageConverterOnHeartbeatedHeard;
        }

        public override void Activate()
        {
            base.Activate();
            _heartBeatMaker.OnSocketError += (sender, args) => HeartbeatNotHeard();
            _heartBeatMaker.Start();
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
        public event VoidMessageEventHandler HeartbeatNotHeard;
    }
}