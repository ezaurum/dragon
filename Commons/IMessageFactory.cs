namespace Dragon
{
    /// <summary>
    /// Need to be singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageFactory<out T> where T : IMessage
    {
        T GetMessage(byte[] bytes);
        T GetMessage(byte[] bytes, int offset, int length);
    }

    /// <summary>
    /// Message Factory. Has Two Generic Template, Request and Acknowlege
    /// Need to be singleton
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public interface IMessageFactory<in TReq, TAck>
    {
        void GetMessage(byte[] bytes, int offset, int length, out TAck ack, out int errorCode);
        void GetByte(TReq req, out byte[] bytes, out int errorCode);
    }
}