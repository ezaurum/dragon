using System;
using System.Collections.Generic;
using System.Linq;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster
    {
        private void ProcessAction()
        {
            long a = 0;
            long actionSum = 0;
            long timeSum = 0;
            long average = 0;

            
            Logger.Debug("Process action");

            foreach (IDragonMarbleGameMessage action in PlayerActions())
            {
                DateTime start = DateTime.Now;

                Board.GrossAssets = 0;
                Units.Values.ToList().ForEach(p => Board.GrossAssets += p.Assets);
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Gross Assets is : {0}", Board.GrossAssets);
                }

                CurrentAction = action;
                _state = GameState.ProcessPlayerAction;
                if (GameMessageType.ActionResultCopy == action.MessageType)
                {
                    _receiveMessageWaitHandler.Reset();
                    _availablePlayers.ForEach(p => p.IsActionResultCopySended = false);
                }

                if (GameMessageType.ActionResultCopy != action.MessageType)
                {
                    Notify(action);
                }
                
                
                if (Logger.IsDebugEnabled)
                {
                    a = (DateTime.Now - start).Milliseconds;
                    actionSum++;
                    timeSum += a;
                    average = timeSum / actionSum;

                    Logger.DebugFormat(" {0} ms, average :{1}ms", a, average);
                }

                //wait for action result copy when message is action result copy
                if (GameMessageType.ActionResultCopy == action.MessageType)
                {
                    Logger.DebugFormat("wait for action result copy Turn:{0}", Turn + 1);
                    _receiveMessageWaitHandler.WaitOne();
                }
            }
        }

        private IEnumerable<IGameMessage> PlayerActions()
        {
            Logger.Debug("Player actions");
            foreach (IGameMessage action
                in PlayersOrderByTurn().SelectMany(player => player.Actions()))
            {
                _state = GameState.WaitPlayerAction;

                Logger.DebugFormat("_state:{0}", _state);

                //turn owner's action
                yield return action;
            }
        }

        private IEnumerable<StageUnitInfo> PlayersOrderByTurn()
        {
            for (Turn = 0; Turn < TurnLimit; Turn++)
            {
                Logger.DebugFormat("start Turn:{0}", Turn + 1);

                Notify(CurrentPlayer.ActivateTurn());

                yield return CurrentPlayer;

                _receiveMessageWaitHandler.Reset();
            }
        }
    }
}
