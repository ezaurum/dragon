namespace Dragon
{
    /// <summary>
    /// Request and Response is same type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageFactory<T> : IMessageFactory<T,T>
    { 

    }

    /// <summary>
    /// Message Factory. Has Two Generic Template, Request and Acknowlege
    /// Need to be singleton
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public interface IMessageFactory<in TReq, TAck>
    {
        void GetByte(TReq req, out byte[] bytes, out int errorCode);
        bool Read(byte[] bytes, int offset, int length, out TAck message, out int errorCode);
    }
}