using DragonMarble.Message;

namespace DragonMarble
{
    public interface IRoomManager
    {
        IDragonMarbleGameMessage RequestNew(byte boardType, GamePlayType playType, byte mode);
        IDragonMarbleGameMessage RequestRandom(byte boardType, GamePlayType playType, byte mode);
    }

    public interface IWaitingRoom
    {
        IDragonMarbleGameMessage Welcome(IDragonMarbleGameMessage message);
        IDragonMarbleGameMessage Ready(IDragonMarbleGameMessage message);
        IDragonMarbleGameMessage Exit(IDragonMarbleGameMessage message);
        IDragonMarbleGameMessage StartGame(IDragonMarbleGameMessage message);
    }
}