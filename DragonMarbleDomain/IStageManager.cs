﻿using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public interface IStageManager
    {
        Guid Id { get; set; }
        Dictionary<Guid, StageUnitInfo> Units { get; set; }
        Dictionary<Int32, StageChanceCardInfo> Cards { get; set; }
        void Notify(IDragonMarbleGameMessage gameActions);
        void Join(StageUnitInfo player);
        void Ban(StageUnitInfo stageUnitInfo);

        //real time checker
        void ActionResultCopySended();
        void OrderSelectSended(OrderCardSelectGameMessage message);
        void ReadyNotify(ReadyStateGameMessage message);
    }
}