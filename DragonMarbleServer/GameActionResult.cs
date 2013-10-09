using System.Collections.Generic;

namespace DragonMarble
{
    public class GameActionResult
    {
        public GameActionResult(GameAction action)
        {
            ActionType = action.Type;
        }

        public GameActionType ActionType { get; set; }
        public GameActionResultType Type { get; set; }
        public bool Success { get; set; }
        public ICollection<int> EffectedPlayers { get; set; }

        public List<StageUnit> TargetUnits { get; set; }
        public List<StageTile> TargetTiles { get; set; }
    }
}