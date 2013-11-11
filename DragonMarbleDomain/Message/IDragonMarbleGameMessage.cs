using Dragon.Message;

namespace DragonMarble.Message
{
    public interface IDragonMarbleGameMessage : IGameMessage
    {
        GameMessageType MessageType { get; }
    }
}