using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    /// Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientDragonSocket<T> : DragonSocket<T> where T : IMessage
    {
        public void Connect() {

        }

        public void Disconnect()
        {

        }        

        private SocketConnector _connector;
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

        public Socket Socket { set; protected get; }
        public SocketAsyncEventArgs WriteEventArgs { set; protected get; }
        public SocketAsyncEventArgs ReadEventArgs { set; protected get; }

        private MessageConverter<T> _messageConverter;

        public void Send(T message)
        {
            WriteEventArgs.SetBuffer(message.ToByteArray(), 0, message.Length);

            if (!Socket.SendAsync(WriteEventArgs))
            {
                //TODO
                WriteCompleted(message);
            }
        }
    }
}
