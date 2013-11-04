using System;
using System.Collections.Generic;

namespace DragonMarble.Game
{
    public class GameRecords
    {
        public Guid Id { get; set; }
        public long HighScoreOnAGame { get; set; }
        public long HighScoreOnAWeek { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
        public int WinningRate { get; set; }
        public int Games { get; set; }
        public List<GameResult> Records { get; set; }
    }
}