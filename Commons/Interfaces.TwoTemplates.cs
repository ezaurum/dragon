using System;

namespace Dragon
{
    /// <summary>
    /// Message Factory. Has Two Generic Template, Request and Acknowlege
    /// Need to be singleton
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TAck"></typeparam>
    public interface IMessageFactory<in TReq,TAck>
    {
        void GetMessage(byte[] bytes, int offset, int length, out TAck ack, out int errorCode);
        void GetByte(TReq req, out byte[] bytes, out int errorCode);
    }

    public interface IDragonSocket<TReq, TAck> :IDisposable
    {        
        event MessageHandler<TAck, int> ReadCompleted; 
        event MessageHandler<TReq> WriteCompleted;
        event VoidMessageEventHandler Disconnected;
        void Send(TReq message);
        void Activate();
        void Deactivate();
    }
}
