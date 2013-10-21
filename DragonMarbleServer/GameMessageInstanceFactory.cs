using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public static class  GameMessageInstanceFactory
    {
        public static InitializeGameGameMessage MakeInitializePlayerMessage(StageUnitInfo p, object[] parameterObjects)
        {
            List<StageUnitInfo> units = (List<StageUnitInfo>)parameterObjects[0];
            List<short> feeBoostedTiles = (List<short>) parameterObjects[1];
            
            return new InitializeGameGameMessage
            {
                To = p.Id,
                FeeBoostedTiles = feeBoostedTiles,
                NumberOfPlayers = (short)units.Count,
                Units = units
            };
        }

        public static ActivateTurnGameMessage ActivateTurn(StageUnitInfo p, object[] arg2)
        {
            return new ActivateTurnGameMessage
              {
                    To = p.Id,
                    TurnOwner = (Guid) arg2[0],
                    ResponseLimit = 50000
          };
        }

        /**
         * <summary>
         * order card selected by client
         * (Int16 NumberOfPlayers, List&gt;bool&lt; OrderCardSelectState, Int16 SelectedCardNumber)
         * </summary>
         * <param name="p">stage unit info</param>
         * <param name="args2[0]">short, NumberOfPlayers</param>
         * <param name="args2[1]">list of short, OrderCardSelectState</param>
         * <param name="args2[2]">short, SelectedCardNumber</param>
         */
        public static OrderCardSelectGameMessage OrderCardSelect(StageUnitInfo p, object[] arg2)
        {
            return new OrderCardSelectGameMessage
            {
                To = p.Id,
                Actor = p.Id,
                NumberOfPlayers = (short)arg2[0],
                OrderCardSelectState = (List<bool>) arg2[1],
                SelectedCardNumber = (short)arg2[2]
            };
        }

        
    }
}