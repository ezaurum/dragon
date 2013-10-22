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

                        Dice.Roll(rollMoveDiceGameMessage.Pressed
                            , rollMoveDiceGameMessage.Odd
                            , rollMoveDiceGameMessage.Even);

                        if (Dice.isDouble)
                        {
                            if (Dice.rollCount > 2)
                            {
                                yield return new GameAction ()
                                {
                                    //TODO prison
									Actor = this,
                                    Type = GameMessageType.ThreeDoubleToPrison,
									ArgObjects = new object[] {this, (char)Dice.result[0], (char)Dice.result[1], (char)Dice.rollCount }
                                    
                                };
                                yield break;
                            }

                            ActionRemined += 1;
                        } 
                        
                        Go(Dice.resultSum);
                        
                        yield return new GameAction()
                        {
                            Actor = this,
                            NeedOther = false,
                            Type = GameMessageType.RollMoveDiceResult,
                            Message = new RollMoveDiceResultGameMessage
                            {
                                Actor = Id,
                                Dices = new List<char> 
                                {(char)Dice.result[0], (char)Dice.result[1]},
                                RollCount =  (char)Dice.rollCount,
                            }
                        };
                        
                        StageTile stageTile = Stage.Tiles[tileIndex];
                        switch (stageTile.Type)
                        {
                            case StageTileInfo.TYPE.CITY:
                            case StageTileInfo.TYPE.SIGHT:
                                yield return new GameAction()
                                {
                                    Actor = this,
                                    Type = GameMessageType.BuyLandRequest,
                                    Message = new BuyLandRequestGameMessage
                                    {
                                        Actor = Id,
                                        ResponseLimit = 50000
                                    }
                                };
                                break;
                        }
                        
                        IDragonMarbleGameMessage afterMovedMessage = ReceivedMessage;
						
                        break;
                }
                DeactivateTurn();
            }
        }
    }
}
