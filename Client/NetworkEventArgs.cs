using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public class AsyncClientUserToken  
    {
        public Boolean Result { get; set; }
        public Socket Socket { get; set; }
    }

    public class SimpleAsyncClientUserToken : AsyncClientUserToken
    {
        public IGameMessage GameMessage { get; set; }
    }

    public class QueueAsyncClientUserToken : AsyncClientUserToken
    {
        private readonly Queue<IGameMessage> _messages = new Queue<IGameMessage>();

        public QueueAsyncClientUserToken()
        {
            Buffer = new byte[1024];
        }

        public IGameMessage Message
        {   
            get
            {
                if (IsEmpty()) return null;
                return _messages.Dequeue();
            }
            set
            {
                if (_messages.Count > 0 && _messages.Last() != value)
                    _messages.Enqueue(value);
            }
        }

        public short MessageLength { get; set; }
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }

        public bool IsEmpty()
        {
            return _messages.Count < 1;
        }
    }
}