﻿using System.Collections.Generic;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public class GameAction : IGameAction
    {
        public GameMessageType Type { get; set; }
        public int Selected { get; set; }
        public int PlayerNumber { get; set; }
        public bool NeedOther { get; set; }
        public int TargetPlayer { get; set; }
        public StageUnitInfo Actor { get; set; }
        public ICollection<StageUnitInfo> TargetUnits { get; set; }
        public ICollection<StageTileInfo> TargetTiles { get; set; }
        public object[] ArgObjects { get; set; }
        public IDragonMarbleGameMessage Message { get; set; }
    }
}