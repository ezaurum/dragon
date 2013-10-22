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

                        yield return Dice.RollAndGetResultGameAction(this
                            , rollMoveDiceGameMessage.Pressed
                            , rollMoveDiceGameMessage.Odd
                            , rollMoveDiceGameMessage.Even);

                        if (Dice.isDouble)
                        {
                            if (Dice.rollCount > 2)
                            {
                                yield return GoToPrison();
                                yield break;
                            }

                            ActionRemined += 1;
                        }
                        
                        Go(Dice.resultSum);

                        var destinationGameAction = DestinationGameAction();
                        if ( null != destinationGameAction)
                            yield return destinationGameAction;

                        break;
                }
                DeactivateTurn();
            }
        }

        private GameAction GoToPrison()
        {
            Go(GameBoard.IndexOfPrison);
            return new GameAction ()
            {
                //TODO prison
                Actor = this,
                Type = GameMessageType.ForceMoveToPrison,
                Message = new ForceMoveToPrisonGameMessage()
            };
        }

        private GameAction DestinationGameAction()
        {
            StageTile stageTile = Stage.Tiles[tileIndex];
            switch (stageTile.Type)
            {
                case StageTileInfo.TYPE.CITY:
                case StageTileInfo.TYPE.SIGHT:
                    return new GameAction()
                    {
                        Actor = this,
                        Type = GameMessageType.BuyLandRequest,
                        Message = new BuyLandRequestGameMessage
                        {
                            Actor = Id,
                            ResponseLimit = 50000
                        }
                    };
                default:
                    return null;
            }
        }
    }
}
