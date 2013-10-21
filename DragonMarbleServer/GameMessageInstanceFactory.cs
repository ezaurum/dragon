using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public static class  GameMessageInstanceFactory
    {
        public static InitializeGameGameMessage MakeInitializePlayerMessage(GamePlayer p, object[] parameterObjects)
        {
            List<StageUnitInfo> units = (List<StageUnitInfo>)parameterObjects[0];
            List<short> feeBoostedTiles = (List<short>) parameterObjects[1];
            
            return new InitializeGameGameMessage
            {
                From = p.StageManager.Id,
                To = p.Id,
                FeeBoostedTiles = feeBoostedTiles,
                NumberOfPlayers = (short)units.Count,
                Units = units
            };
        }

        public static ActivateTurnGameMessage ActivateTurn(GamePlayer p, object[] arg2)
        {
            return new ActivateTurnGameMessage
              {
                    To = p.Id,
                    From = p.StageManager.Id,
                    TurnOwner = (Guid) arg2[0],
                    ResponseLimit = 50000
          };
        }
    }
}