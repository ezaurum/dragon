using System.Net.Sockets;

namespace Dragon
{
    /// <summary>
    ///     Socket Wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DragonSocket<T> : IDragonSocket<T> where T : IMessage
    {
        private readonly MessageConverter<T> _messageConverter;

        protected DragonSocket(IMessageFactory<T> factory)
        {
            _messageConverter = new MessageConverter<T>(new CircularBuffer(new byte[1024]), factory);
            ReadEventArgs = new SocketAsyncEventArgs();

            //TODO is this need pool?
            ReadEventArgs.SetBuffer(new byte[1024], 0, 1024);
            ReadEventArgs.Completed += OnReadEventArgsOnCompleted;

            WriteEventArgs = new SocketAsyncEventArgs();
            WriteEventArgs.Completed += OnWriteEventArgsOnCompleted;
        }

        public event MessageEventHandler<T> ReadCompleted
        {
            add { _messageConverter.MessageConverted += value; }
            remove { _messageConverter.MessageConverted -= value; }
        }

        public event MessageEventHandler<T> WriteCompleted;
        public event MessageEventHandler<T> Disconnected;

        //TODO event disconnected
        //TODO event connected

        public Socket Socket { set; protected get; }
        public SocketAsyncEventArgs WriteEventArgs { set; protected get; }
        public SocketAsyncEventArgs ReadEventArgs { set; protected get; }

        public void Send(T message)
        {
            WriteEventArgs.SetBuffer(message.ToByteArray(), 0, message.Length);
            WriteEventArgs.UserToken = message;

            if (!Socket.SendAsync(WriteEventArgs))
            {
                OnWriteEventArgsOnCompleted(null, WriteEventArgs);
            }
        }

        private void OnWriteEventArgsOnCompleted(object sender, SocketAsyncEventArgs e)
        {
            WriteCompleted((T) e.UserToken);
        }

        /// <summary>
        ///     Read repeat
        /// </summary>
        protected void ReadRepeat()
        {
            if (!Socket.ReceiveAsync(ReadEventArgs))
            {
                OnReadEventArgsOnCompleted(Socket, ReadEventArgs);
            }
        }

        /// <summary>
        ///     Default receive arg complete event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnReadEventArgsOnCompleted(object sender, SocketAsyncEventArgs args)
        {
            //TODO error process need
            if (args.SocketError != SocketError.Success)
            {
                Disconnected(default(T));
            }

            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                _messageConverter.ReceiveBytes(args.Buffer, args.Offset, args.BytesTransferred);
                ReadRepeat();
            }
        }
    }
}