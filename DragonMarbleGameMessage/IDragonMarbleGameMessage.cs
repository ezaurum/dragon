using Dragon.Interfaces;

namespace DragonMarble.Message
{
    public interface IDragonMarbleGameMessage : IGameMessage
    {
        GameMessageType MessageType { get; }
    }
}