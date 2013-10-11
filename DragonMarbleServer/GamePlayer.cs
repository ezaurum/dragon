using System;
using log4net;

namespace DragonMarble
{
    public class GamePlayer :StageUnit
    {
        
        
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GamePlayer));

        public StageUnit Unit { get; set; }
        public GameMaster GameMaster { get; set; }
        public Guid Id { get; set; }

        public GamePlayer() 
            :base(StageUnitInfo.TEAM_COLOR.RED, 1000000)
        {
            Id = Guid.NewGuid();
        }
    }
}