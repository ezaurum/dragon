using System.Collections.Generic;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
	public class AIStageUnitInfo : StageUnitInfo {

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
			case GameMessageType.RollMoveDiceResult:
				
				
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

        public IEnumerable<IDragonMarbleGameMessage> GetMessageResult(IDragonMarbleGameMessage receivedMessage)
	    {
            if (!OwnTurn) yield break;
	    }

		public IEnumerable<IGameAction> Actions ()
		{

			for (ActionRemined = 1; ActionRemined > 0; ActionRemined--) {
				IDragonMarbleGameMessage receivedMessage = ReceivedMessage;

				switch (specialState) {
				case SPECIAL_STATE.NONE:
					var rollMoveDiceGameMessage = (RollMoveDiceGameMessage)receivedMessage;
					yield return Dice.RollAndGetResultGameAction(this
                            , rollMoveDiceGameMessage.Pressed
                            , rollMoveDiceGameMessage.Odd
                            , rollMoveDiceGameMessage.Even);

					if (Dice.isDouble) {
						if (Dice.rollCount > 2) {
							Prison();
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
					
				case SPECIAL_STATE.PRISON:
					PrisonActionGameMessage prisonActionMsg = (PrisonActionGameMessage) receivedMessage;
					Dice.Roll();
					//yield return Dice.RollAndGetResultGameAction(this, 0.5f, false, false);
					bool escape = false;
					switch ( prisonActionMsg.ActionIndex ){
					case (char)PRISON_ACTION.ROLL:
						if ( Dice.isDouble ){
							escape = true;
						}else{
							UpdatePrisonState();
						}
						break;
					case (char)PRISON_ACTION.PAY:
						if ( AddGold( - GameBoard.PRISON_PRICE ) ){
							if (Dice.isDouble) {
								ActionRemined += 1;
							}
							escape = true;
						}else{
							SelfBan();
						}
						break;
					case (char)PRISON_ACTION.CARD:
						if ( chanceCoupon == CHANCE_COUPON.ESCAPE_ISLAND ){
							chanceCoupon = CHANCE_COUPON.NONE;
							escape = true;
						}else{
							SelfBan();
						}
						break;
					}
				        yield return new PrisonActionResultGameMessage
				        {
				            Actor = Id,
				            EscapeResult = escape,
				            EscapeType = prisonActionMsg.ActionIndex
				        };
				        yield return new RollMoveDiceResultGameMessage
				        {
				            Actor = Id,
				            Dices = new List<char> {(char) Dice.result[0], (char) Dice.result[1]}
				        };
					
					if ( escape ){
						specialState = SPECIAL_STATE.NONE;
						specialStateValue = 0;
						Go (Dice.resultSum);
						foreach(var destinationGameAction in DestinationGameAction () ) {
							if (null != destinationGameAction)
								yield return destinationGameAction;
						}	
					}
					break;
					
				case SPECIAL_STATE.TRAVEL:
					TravelActionGameMessage travelActionMsg = (TravelActionGameMessage) receivedMessage;
					if ( tileIndex == travelActionMsg.TileIndex ){
						SelfBan();
					}else{
						tileIndex = travelActionMsg.TileIndex;
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
		
		private IEnumerable<IGameAction> DestinationGameAction ()
		{
			StageTileInfo stageTile = Stage.Tiles [tileIndex];
			switch ( stageTile.type ) {
			case StageTileInfo.TYPE.CITY:
			case StageTileInfo.TYPE.SIGHT:
                foreach (var gameAction in MoveResultCitySight(stageTile)) yield return gameAction;
			        break;
			case StageTileInfo.TYPE.PRISON:
				ActionRemined = 0;
				Prison();
				yield return null;
				break;
				
			default:
				yield return null;
				break;
			}
		}

	    private IEnumerable<IGameAction> MoveResultCitySight(StageTileInfo stageTile)
	    {
	        if (stageTile.IsAbleToBuy(this))
	        {
	            yield return new BuyLandRequestGameMessage
	            {
	                Actor = Id,
	                ResponseLimit = 50000
	            };
	            IDragonMarbleGameMessage receivedMessage = ReceivedMessage;
	            if (BuyLand(receivedMessage))
	            {
	                yield return receivedMessage;
	            }
	            else
	            {
	                SelfBan();
	            }
	        }
	        else
	        {
	            if (stageTile.owner != null && !stageTile.IsSameOwner(this))
	            {
	                if (Pay(stageTile))
	                {
	                    yield return new PayFeeGameMessage
	                    {
	                        Actor = Id
	                    };
	                }
	                if (stageTile.IsAbleToTakeover(this))
	                {
	                    yield return new TakeoverRequestGameMessage
	                    {
	                        Actor = Id
	                    };

	                    IDragonMarbleGameMessage receivedMessage = ReceivedMessage;
	                    if (Takeover(receivedMessage))
	                    {
	                        yield return receivedMessage;

	                        if (stageTile.IsAbleToBuy(this))
	                        {
	                            yield return new BuyLandRequestGameMessage
	                            {
	                                Actor = Id,
	                                ResponseLimit = 50000
	                            };
	                            receivedMessage = ReceivedMessage;
	                            if (BuyLand(receivedMessage))
	                            {
	                                yield return receivedMessage;
	                            }
	                            else
	                            {
	                                SelfBan();
	                            }
	                        }
	                    }
	                    else
	                    {
	                        SelfBan();
	                    }
	                }
	            }
	        }
	        yield return null;
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
		
		private bool Takeover(IDragonMarbleGameMessage receivedMessage){
			TakeoverGameMessage msg = (TakeoverGameMessage) receivedMessage;
			if ( msg.Takeover ) {
				return Stage.Tiles[this.tileIndex].TakeOver( this );
			}
			return true;
		}
		
		private void SelfBan(){
			
		}
		
	}
}