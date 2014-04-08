namespace Dragon
{
    public class ConcurrentDragonSocket<TReq, TAck> : DragonSocket<TReq, TAck>
    {
        public ConcurrentDragonSocket(IMessageConverter<TReq, TAck> converter, byte[] buffer, int offset, int bufferSize) 
            : base(converter, buffer, offset, bufferSize)
        {

        }
    }
}