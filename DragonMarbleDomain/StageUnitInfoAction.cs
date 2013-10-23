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
							break;
						}

						ActionRemined += 1;
					}
                        
					Go (Dice.resultSum);
					
					foreach(var destinationGameAction in DestinationGameAction () ) {
						if (null != destinationGameAction)
							yield return destinationGameAction;
					}

					break;
				}
			}
			DeactivateTurn ();
		}

		private GameAction GoToPrison ()
		{
			Prison();
			//Go (Stage.TILE_INDEX_PRISON);
			return new GameAction ()
            {
                Actor = this,
                Type = GameMessageType.ForceMoveToPrison,
                Message = new ForceMoveToPrisonGameMessage ()
            };
		}
		
		private IEnumerable<GameAction> DestinationGameAction ()
		{
			StageTileInfo stageTile = Stage.Tiles [tileIndex];
			switch ( stageTile.type ) {
			case StageTileInfo.TYPE.CITY:
			case StageTileInfo.TYPE.SIGHT:
				if ( stageTile.IsAbleToBuy( this ) ) {
					yield return new GameAction ()
                    {
                        Actor = this,
                        Type = GameMessageType.BuyLandRequest,
                        Message = new BuyLandRequestGameMessage
                        {
                            Actor = Id,
                            ResponseLimit = 50000
                        }
                    };
					IDragonMarbleGameMessage receivedMessage = ReceivedMessage;
					switch (receivedMessage.MessageType) {
					case GameMessageType.BuyLand:
					{
						if ( BuyLand(receivedMessage) != true ){
							SelfBan();
						}
						
						break;
					}
					}
					
				} else {
					if ( !stageTile.IsSameOwner( this ) ){
						if ( this.Pay( stageTile ) ){
							yield return new GameAction ()
	                        {
	                            Actor = this,
	                            Type = GameMessageType.PayFee,
	                            Message = new PayFeeGameMessage
	                            {
	                                Actor = Id,
									TileIndex = (char)stageTile.index
	                            }
	                        };
						}
						
					}
					
				}
				yield return null;
				break;
			default:
				yield return null;
				break;
			}
		}
		
		
		private bool BuyLand(IDragonMarbleGameMessage receivedMessage){
			BuyLandGameMessage msg = (BuyLandGameMessage) receivedMessage;
			if ( msg.Buildings == StageTileInfo.BUILDING[4] ){
				return Stage.Tiles[msg.TileIndex].BuyLandmark(this);
			}
			List<int> buildingIndex = new List<int>();
			for ( int i = 0; i < 3; i++ ){
				if ( StageTileInfo.BUILDING[i] == ( StageTileInfo.BUILDING[i] & msg.Buildings ) ){
					buildingIndex.Add(i);
				}
			}
			return Stage.Tiles[msg.TileIndex].Buy(this, buildingIndex);
		}
		
		private void SelfBan(){
			
		}
		
	}
}