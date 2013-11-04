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
        TeamPlay,
        Individual2PlayerPlay,
        Individual3PlayerPlay,
        Individual4PlayerPlay,
        Random
    }
}