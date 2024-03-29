using System;

namespace Dragon
{
    public interface IBeatable<T>
    {
        bool HeartBeatEnable { get; set; }
        T HeartBeatMessage { get; set; }
        event Action<T> UpdateMessage;
    }
}