using System;

namespace Dragon
{
    public interface IMessageSender<in TReq>
    {
        void Send(TReq message);
        event Action<int> WriteCompleted;
    }
}