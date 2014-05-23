using System;

namespace Dragon
{
    [Flags]
    public enum SocketState : byte
    { 
        Disposed = 0,
        Connectiong = 0x1,
        Connected = 0x2,
        Initialized = 0x4,
        Active = Connected | Initialized, 
    }
}