using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class StageUnitInfo
    {
        public void ActivateTurn()
        {
            OwnTurn = true;
        }

        public void DeactivateTurn()
        {
            OwnTurn = false;
            Dice.Clear();
        }

        public IEnumerable<GameAction> Actions()
        {
            for (ActionRemined = 1; ActionRemined > 0; ActionRemined--)
            {
                //wait until player request a action
                IDragonMarbleGameMessage receivedMessage = ReceivedMessage;

                switch (receivedMessage.MessageType)
                {
                    case GameMessageType.RollMoveDice:
                        var rollMoveDiceGameMessage = (RollMoveDiceGameMessage) receivedMessage;

                        var rmdrgm = new RollMoveDiceResultGameMessage
                        {
                            From = StageManager.Id,
                            To = Id,
                            Dices = new List<Char> {(char) Dice.result[0], (char) Dice.result[1]}
                        };

                        if (Dice.isDouble)
                        {
                            if (Dice.rollCount > 2)
                            {
                                yield return new GameAction ()
                                {
                                    //TODO prison
                                    Type = GameMessageType.RollMoveDiceResult
                                    
                                };
                                yield break;
                            }

                            ActionRemined += 1;
                        } 
                        
                        Go(Dice.resultSum);

                        GameAction action = new GameAction()
                        {
                            Actor = this,
                            NeedOther = false,
                            Type = GameMessageType.RollMoveDiceResult,
                        };

                        yield return action;

                        //
                        IDragonMarbleGameMessage afterMovedMessage = ReceivedMessage;


                        break;
                }
                DeactivateTurn();
            }
        }
    }
}
