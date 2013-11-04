namespace DragonMarble
{
    /// <summary>
    /// Grade of Game Items such as Character Card, Fourtune Item, Dice ...
    /// </summary>
    public enum GameItemGrade
    {
        F = 0,
        E,
        EP,
        EPP,
        C,
        CP,
        CPP,
        B,
        BP,
        BPP,
        A,
        AP,
        APP,
        S,
        SP,
        SPP
    }

    public enum WinConditionType
    {
        Bankruptcy,
        MonopolyTriple,
        MonopolySight,
        MonopolyLine
    }

    public enum GamePlayType
    {
        TeamPlay = 0,
        Random = 1,
        Individual2PlayerPlay = 2,
        Individual3PlayerPlay = 3,
        Individual4PlayerPlay = 4,
    }
}