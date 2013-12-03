namespace Dragon
{
    /// <summary>
    /// Client Socket. Able to connect remote host.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServerDragonSocket<T> : DragonSocket<T> where T : IMessage
    {
        protected ServerDragonSocket(IMessageFactory<T> factory) : base(factory)
        {
        }

        public event MessageEventHandler<T> Accepted;
    }
}