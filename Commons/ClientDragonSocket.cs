using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Dragon
{
    /// <summary>
    /// Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientDragonSocket<T> : IDragonSocket<T> where T : IMessage
    {
        public void Connect() {

        }

        public void Disconnect()
        {

        }        

        private SocketConnector _connector;
        public event MessageEventHandler<T> ReadCompleted;
        public event MessageEventHandler<T> WriteCompleted;
        public Socket Socket { set; }
        public SocketAsyncEventArgs WriteEventArgs { set; }
        public SocketAsyncEventArgs ReadEventArgs { set; }

        private MessageConverter<T> _messageConverter;

        public void Send(T message)
        {
            WriteEventArgs.SetBuffer(message.ToByteArray());

            if (!Socket.SendAsync(Socket, WriteEventArgs))
            {
                //TODO
                WriteCompleted(message);
            }
        }
    }

    /// <summary>
    /// Socket Wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DragonSocket<T> : IDragonSocket<T> where T : IMessage
    {
        public event MessageEventHandler<T> ReadCompleted;
        public event MessageEventHandler<T> WriteCompleted;
        
        //TODO event disconnected
        //TODO event connected

        public Socket Socket { set; }
        public SocketAsyncEventArgs WriteEventArgs { set; }
        public SocketAsyncEventArgs ReadEventArgs { set; }

        private MessageConverter<T> _messageConverter;

        public void Send(T message)
        {
            WriteEventArgs.SetBuffer(message.ToByteArray());

            if (!Socket.SendAsync(Socket, WriteEventArgs))
            {
                //TODO
                WriteCompleted(message);
            }
        }
    }
}
