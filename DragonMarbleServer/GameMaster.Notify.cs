using System;
using System.Collections.Generic;
using System.Net.Sockets;
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

        /// <summary>
        /// after Connect event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void AddPlayer(object sender, SocketAsyncEventArgs e)
        {
            Raja token = (Raja)e.UserToken;
            token.Unit = new StageUnitInfo
            {
                Id = Guid.NewGuid(),
                Order = 0,
                UnitColor = StageUnitInfo.UNIT_COLOR.BLUE,
                CharacterId = 1,
                Gold = 2000000
            };

            GameMaster gm = new GameMaster(2);

            gm.Join(token.Unit);

            StageUnitInfo dummyAI = new AIStageUnitInfo()
            {
                Id = Guid.NewGuid(),
                Order = 1,
                UnitColor = StageUnitInfo.UNIT_COLOR.GREEN,
                CharacterId = 1,
                Gold = 2000000,
                ControlMode = StageUnitInfo.ControlModeType.AI_0
            };
         // gm.Join(dummyAI);
        }
    }
}
