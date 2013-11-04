using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster : IStageManager
    {
        public Dictionary<Guid, StageUnitInfo> Units { get; set; }
        public Guid Id { get; set; }
        /// <summary>
        ///     Notify message for every players
        /// </summary>
        /// <param name="message">message to notify</param>
        public void Notify(IDragonMarbleGameMessage message)
        {
            foreach (StageUnitInfo stageUnitInfo in Units.Values)
            {
                stageUnitInfo.SendingMessage = message;
            }
        }

        /// <summary>
        /// ban from game
        /// </summary>
        /// <param name="stageUnitInfo"></param>
        public void Ban(StageUnitInfo stageUnitInfo)
        {
            Logger.DebugFormat("banned. :{0}",stageUnitInfo.Id);
            stageUnitInfo.ControlMode= StageUnitInfo.ControlModeType.AI_0;
            Raja raja = (Raja) stageUnitInfo.MessageProcessor;
            raja.Dispose();
        }
    }
}
