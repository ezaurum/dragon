using System;
using System.Collections.Generic;

namespace DragonMarble
{
    public class GameAccountInfo
    {
        private GameAccountStat _stat;
        public Guid Id { get; set; }
        public int Level { get; set; }
        public int TotalExp { get; set; }
        //number of games played
        public int TotalGames { get; set; }
        public String Name { get; set; }
        public String GreetingWords { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime Joined { get; set; }
        public DateTime LastPlay { get; set; }
        
        public int RankingWithFriends { get; set; }
        public int RankingPercentWithEverybody { get; set; }
        public long RankingWithEverybody { get; set; }
        public long GameMoney { get; set; }
        public long CashMoney { get; set; }
        public int GamePlayEnergy  { get; set; }
        public int PresentSended { get; set; }
        //public long HighScore

        public GameRecords ThisWeekGameRecords { get; set; }
        public GameRecords GameRecords { get; set; }
        
    }

    public class GameRecords
    {
        public long HighScoreOnAGame { get; set; }
        public long HighScoreOnAWeek { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
        public int WinningRate { get; set; }
        public int Games { get; set; }
        public List<GameResult> Records { get; set; }
    }

    public class GameResult
    {
        public List<GameAccountInfo> Players { get; set; }
        public GameAccountInfo Winner { get; set; }
        public long Prize { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public GamePlayType Type { get; set; }
        public TimeSpan PlayTime
        {
            get { return StartTime - EndTime; }
        }
        public WinConditionType WinCondition{ get; set; }

        
    }

    public enum WinConditionType
    {
        Bankruptcy, MonopolyTriple, MonopolySight, MonopolyLine
    }

    public enum GamePlayType
    {
        TeamPlay, Individual2PlayerPlay, Individual3PlayerPlay, Individual4PlayerPlay, Random
    }

    public class GameAccountStat
    {
        //계정 레벨
        public int Level { get; set; }
        //다음 레벨업 필요 경험치
        public int ExpNeededForLevelUp { get; set; }
        //총 경험치	
        public int TotalExp { get; set; }
        //레벨 보너스
        public int RankingBonusPoint { get; set; }
    }
}