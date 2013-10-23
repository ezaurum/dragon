using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
	public class AIStageUnitInfo : StageUnitInfo {
		
		 public override IDragonMarbleGameMessage ReceivedMessage
        {
            get { return MessageProcessor.ReceivedMessage; }
        }

        public override IDragonMarbleGameMessage SendingMessage
        {
            set { 
				MessageProcessor.SendingMessage = value;
				AIMessageProcess(value);
			}
        }
		
		private void AIMessageProcess(IDragonMarbleGameMessage message) {
			switch ( message.MessageType ) {
			case GameMessageType.ActivateTurn:
				ReceivedMessage = new RollMoveDiceGameMessage() {
					Actor = Id,
					Pressed = RandomUtil.Next(0f, 1f)
				};
				break;
			}
		}
	}
	public partial class StageUnitInfo
	{
		public IDragonMarbleGameMessage ActivateTurn ()
		{
			OwnTurn = true;
			IDragonMarbleGameMessage message = new ActivateTurnGameMessage
            {
                TurnOwner = Id,
                ResponseLimit = 50000
            };

			return message;
		}

		public void DeactivateTurn ()
		{
			OwnTurn = false;
			Dice.Clear ();
		}

		public IEnumerable<GameAction> Actions ()
		{
			for (ActionRemined = 1; ActionRemined > 0; ActionRemined--) {
				//wait until player request a action
				IDragonMarbleGameMessage receivedMessage = ReceivedMessage;

				switch (receivedMessage.MessageType) {
				case GameMessageType.RollMoveDice:
					var rollMoveDiceGameMessage = (RollMoveDiceGameMessage)receivedMessage;

					yield return Dice.RollAndGetResultGameAction(this
                            , rollMoveDiceGameMessage.Pressed
                            , rollMoveDiceGameMessage.Odd
                            , rollMoveDiceGameMessage.Even);

					if (Dice.isDouble) {
						if (Dice.rollCount > 2) {
							yield return GoToPrison();
							yield break;
						}

						ActionRemined += 1;
					}
                        
					Go (Dice.resultSum);

					var destinationGameAction = DestinationGameAction ();
					if (null != destinationGameAction)
						yield return destinationGameAction;

					break;
				}
				DeactivateTurn ();
			}
		}

		private GameAction GoToPrison ()
		{
			Go (GameBoard.IndexOfPrison);
			return new GameAction ()
            {
                Actor = this,
                Type = GameMessageType.ForceMoveToPrison,
                Message = new ForceMoveToPrisonGameMessage ()
            };
		}
		
		private GameAction DestinationGameAction ()
		{
			StageTileInfo stageTile = Stage.Tiles [tileIndex];
			switch (stageTile.type) {
			case StageTileInfo.TYPE.CITY:
			case StageTileInfo.TYPE.SIGHT:
				if (stageTile.owner == null || stageTile.owner.Equals(this)) {
					return new GameAction ()
                        {
                            Actor = this,
                            Type = GameMessageType.BuyLandRequest,
                            Message = new BuyLandRequestGameMessage
                            {
                                Actor = Id,
                                ResponseLimit = 50000
                            }
                        };
				} else {

					
				}
				return null;

			default:
				return null;
			}
		}
		
		private void AI_BuyLand(StageTileInfo tile){
			StageUnitInfo unit = this;
			if ( tile.owner == null || tile.owner.Equals( unit ) ){
				if ( tile.isAbleToBuildLandmark ){
					tile.BuyLandmark(unit);
				}else{
					List<int> buildingIndex = new List<int>();
					int totalPrice = 0;
					for ( int i = 0; i < tile.buildings.Count; i++ ){
						if ( i > unit.round || i >= 4 ) break;
						
						if ( !tile.buildings[i].isBuilt ){
							if ( totalPrice + tile.buildings[i].buyPrice > unit.gold ){
								break;
							}
							totalPrice += tile.buildings[i].buyPrice;
							buildingIndex.Add( i );
						}
					}
					if ( buildingIndex.Count > 0 ){
						tile.Buy(unit, buildingIndex);
					}
				}
			}
		}
		
		
	}
}