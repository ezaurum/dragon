using System;
using System.Collections.Generic;
using DragonMarble.Card;
using DragonMarble.Game;

namespace DragonMarble.Account
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
        public int GamePlayEnergy { get; set; }
        public int PresentSended { get; set; }
        //public long HighScore

        public GameRecords ThisWeekGameRecords { get; set; }
        public GameRecords GameRecords { get; set; }

        //friends
        public List<GameAccountInfo> Friends { get; set; }
        //dices
        public List<StageDiceInfo> Dieces { get; set; }
        public StageDiceInfo CurrentDice { get; set; }

        //CharacterCards
        public List<CharacterCardInfo> CharacterCards { get; set; }
        public CharacterCardInfo CurrentCharacterCard { get; set; }

        //items
        public List<FortuneItem> FortuneItems { get; set; }

    }

    public class FortuneItem
    {
        public Guid Id { get; set; }
        public GameItemGrade Grade { get; set; }
        public GameAccountInfo Owner { get; set; }
    }

    
    public class GameAccountStat
    {
        public Guid Id { get; set; }
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