using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public class GameAction
    {
        public GameMessageType Type { get; set; }
        public int Selected { get; set; }
        public int PlayerNumber { get; set; }
        public bool NeedOther { get; set; }
        public int TargetPlayer { get; set; }
        public StageUnit Actor { get; set; }
        public ICollection<StageUnit> TargetUnits { get; set; }
        public ICollection<StageTile> TargetTiles { get; set; }
    }
}