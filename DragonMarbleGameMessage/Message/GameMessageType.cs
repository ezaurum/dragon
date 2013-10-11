namespace DragonMarble.Message
{
    public enum GameMessageType
    {
        //system message
        HeartBeat = 0,
        InitUser = 1,
        
        //game 
        InitilizeBoard = 100,

        //user action
        RollMoveDice = 1000,
        OrderCardSelect,
        RollDice,
        RollAutoDice,
        SelectDestination,
        BuySite,
        BuyBuilding,
        TakeOverLand,
        PayMethod,
        AskYesNo,
    }
}