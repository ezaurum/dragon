using DragonMarble.Message;

namespace DragonMarble
{
    public interface IRoomManager
    {
        IDragonMarbleGameMessage RequestNew(GameBoardType boardType, GamePlayType playType, GamePlayMode mode);
        IDragonMarbleGameMessage RequestRandom(GameBoardType boardType, GamePlayType playType, GamePlayMode mode);
    }

    public interface IWaitingRoom
    {
        IDragonMarbleGameMessage Welcome(IDragonMarbleGameMessage message);
        IDragonMarbleGameMessage Ready(IDragonMarbleGameMessage message);
        IDragonMarbleGameMessage Exit(IDragonMarbleGameMessage message);
        IDragonMarbleGameMessage StartGame(IDragonMarbleGameMessage message);
    }
}