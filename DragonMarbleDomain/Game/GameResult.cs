using System;
using System.Collections.Generic;
using DragonMarble.Account;

namespace DragonMarble.Game
{
    public class GameResult
    {
        public Guid Id { get; set; }
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
}