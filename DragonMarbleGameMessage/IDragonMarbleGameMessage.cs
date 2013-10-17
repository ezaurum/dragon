using System;
using Dragon.Interfaces;

namespace DragonMarble.Message
{
    public interface IDragonMarbleGameMessage : IGameMessage
    {
        GameMessageType MessageType { get; }
        Guid To { get; set; }
        Guid From { get; set; }
    }
}