using System;

namespace Dragon
{
    /// <summary>
    /// Message Factory. Has Two Generic Template, Request and Acknowlege
    /// Need to be singleton
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public interface IMessageFactory<TReq,TAck>
    {
        void GetMessage(byte[] bytes, out TReq req);
        void GetMessage(byte[] bytes, out TAck ack);
        void GetMessage(byte[] bytes, int offset, int length, out TReq req);
        void GetMessage(byte[] bytes, int offset, int length, out TAck ack);
        
        void GetByte(TReq req, out byte[] bytes);
        void GetByte(TAck ack, out byte[] bytes);
    }

    public interface IDragonSocket<TReq, TAck> :IDisposable
    {        
        event MessageHandler<TAck> ReadCompleted;
        event MessageHandler<TReq> WriteCompleted;
        event VoidMessageEventHandler Disconnected;
        void Send(TReq message);
        void Activate();
        void Deactivate();
    }
}
