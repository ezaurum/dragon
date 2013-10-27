using Dragon.Message;

namespace DragonMarble.Message
{
    public interface IDragonMarbleGameMessage : IGameMessage, IGameAction
    {
        GameMessageType MessageType { get; }
    }

    public interface IDragonMarbleMessageProcessor : IMessageProcessor<IDragonMarbleGameMessage>
    {
    }
}