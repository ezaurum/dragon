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
            {
                Accepted(Socket, null);
            } 
        }

        private long _lastBeat;
        
        public event EventHandler<SocketAsyncEventArgs> Accepted;

        public void CheckBeat(HeartBeatChecker checker, long obj)
        {
            if (_lastBeat >= obj - 2)
            {
                if (_lastBeat > obj) _lastBeat = obj;
                return;
            }
            checker.OnBeat -= CheckBeat;
            Disconnect();
        }
        
    }
}