using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public class GameActionResult
    {
        public GameActionResult(GameAction action)
        {
            ActionType = action.Type;
        }

        public GameMessageType ActionType { get; set; }
        public bool Success { get; set; }
        public ICollection<int> EffectedPlayers { get; set; }

        public List<GamePlayer> TargetUnits { get; set; }
        public List<StageTile> TargetTiles { get; set; }
    }
}