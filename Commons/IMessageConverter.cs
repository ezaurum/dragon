using System;

namespace Dragon
{
    public interface IMessageConverter<T> where T : IMessage
    {
        event Action<T> MessageConverted;
    }
}