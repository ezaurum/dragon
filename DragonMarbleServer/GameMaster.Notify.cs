using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster : IStageManager
    {
        public List<StageUnitInfo> Units { get; set; }
        public Guid Id { get; set; }
        /// <summary>
        ///     Notify message for every players
        /// </summary>
        /// <param name="message">message to notify</param>
        public void Notify(IDragonMarbleGameMessage message)
        {
            Units.ForEach(p => p.SendingMessage = message);
        }

        /// <summary>
        /// Join player to game.
        /// </summary>
        /// <param name="player"></param>
        public void Join(StageUnitInfo player)
        {
            Units.Add(player);
            player.StageManager = this;
            player.Stage = Board;

            //set initailize player message
            player.SendingMessage = new InitializePlayerGameMessage
            {
                PlayerId = player.Id,
                Server = Id
            };
        }

        public void Ban(StageUnitInfo stageUnitInfo)
        {
            Logger.DebugFormat("banned. :{0}",stageUnitInfo.Id);
            stageUnitInfo.ControlMode= StageUnitInfo.ControlModeType.AI_0;
            Raja raja = (Raja) stageUnitInfo.MessageProcessor;
            raja.Dispose();
        }
    }
}
