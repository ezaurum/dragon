using System.Collections.Generic;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class StageUnitInfo
    {
        public bool _isActionResultCopySended;

        public bool IsActionResultCopySended
        {
            get
            {
                return _isActionResultCopySended;
            }
            set
            {
                _isActionResultCopySended = value;
                if (value)
                {
                    StageManager.ActionResultCopySended();
                }
            }
        }

        public IDragonMarbleGameMessage ActivateTurn ()
		{
			OwnTurn = true;
			IDragonMarbleGameMessage message = new ActivateTurnGameMessage
            {
                TurnOwner = Id,
                ResponseLimit = 5000
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

        public IEnumerable<IGameMessage> Actions()
		{
			for (ActionRemined = 1; ActionRemined > 0; ActionRemined--) {
				IDragonMarbleGameMessage receivedMessage = ReceivedMessage;

				switch (specialState) {
				case SPECIAL_STATE.NONE:
					foreach (var gameMessage in NormalPositionActions(receivedMessage)) 
                        yield return gameMessage;
				        break;
					
				case SPECIAL_STATE.PRISON:
					foreach (var gameMessage1 in InPrisonActions(receivedMessage)) yield return gameMessage1;
				        break;
					
				case SPECIAL_STATE.TRAVEL:
                    foreach (var gameMessage2 in InTravelActions(receivedMessage)) yield return gameMessage2;
                        break;
				    
				}
			}
			DeactivateTurn ();
		}

        private IEnumerable<IGameMessage> InTravelActions(IDragonMarbleGameMessage receivedMessage)
        {
            IDragonMarbleGameMessage travelActionMsg = receivedMessage;
            if (travelActionMsg.MessageType == GameMessageType.TravelAction)
            {
                TravelActionGameMessage msg = (TravelActionGameMessage) travelActionMsg;
                yield return msg;
                if (tileIndex == msg.TileIndex || msg.TileIndex < 0 || msg.TileIndex > 32)
                {
                    SelfBan();
                }
                else
                {
                    tileIndex = msg.TileIndex;
                }
            }
            else if (travelActionMsg.MessageType == GameMessageType.RollMoveDice)
            {
                RollMoveDiceGameMessage msg = (RollMoveDiceGameMessage) travelActionMsg;
                yield return Dice.RollAndGetResultGameAction(this, msg.Pressed, msg.Odd, msg.Even);
                Go(Dice.resultSum);
            }
            specialState = SPECIAL_STATE.NONE;
            specialStateValue = 0;
            foreach (var destinationGameAction in DestinationGameAction())
            {
                if (null != destinationGameAction)
                    yield return destinationGameAction;
            }
        }

        private IEnumerable<IGameMessage> InPrisonActions(IDragonMarbleGameMessage receivedMessage)
        {
            PrisonActionGameMessage prisonActionMsg = (PrisonActionGameMessage) receivedMessage;
            Dice.Roll();
            //yield return Dice.RollAndGetResultGameAction(this, 0.5f, false, false);
            bool escape = false;
            switch (prisonActionMsg.ActionIndex)
            {
                case (char) PRISON_ACTION.ROLL:
                    if (Dice.isDouble)
                    {
                        escape = true;
                    }
                    else
                    {
                        UpdatePrisonState();
                    }
                    break;
                case (char) PRISON_ACTION.PAY:
                    if (AddGold(- GameBoard.PRISON_PRICE))
                    {
                        if (Dice.isDouble)
                        {
                            ActionRemined += 1;
                        }
                        escape = true;
                    }
                    else
                    {
                        SelfBan();
                    }
                    break;
                case (char) PRISON_ACTION.CARD:
                    if (chanceCoupon == CHANCE_COUPON.ESCAPE_ISLAND)
                    {
                        chanceCoupon = CHANCE_COUPON.NONE;
                        escape = true;
                    }
                    else
                    {
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

            if (escape)
            {
                specialState = SPECIAL_STATE.NONE;
                specialStateValue = 0;
                Go(Dice.resultSum);
                foreach (var destinationGameAction in DestinationGameAction())
                {
                    if (null != destinationGameAction)
                        yield return destinationGameAction;
                }
            }
        }

        private IEnumerable<IGameMessage> NormalPositionActions(IDragonMarbleGameMessage receivedMessage)
        {
            var rollMoveDiceGameMessage = (RollMoveDiceGameMessage) receivedMessage;
            yield return Dice.RollAndGetResultGameAction(this
                , rollMoveDiceGameMessage.Pressed
                , rollMoveDiceGameMessage.Odd
                , rollMoveDiceGameMessage.Even);

            if (Dice.isDouble)
            {
                if (Dice.rollCount > 2)
                {
                    Prison();
                    yield break;
                }
                ActionRemined += 1;
            }
            Go(Dice.resultSum);
            foreach (var destinationGameAction in DestinationGameAction())
            {
                yield return destinationGameAction;
            }
        }

        private IEnumerable<IGameMessage> DestinationGameAction()
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
                    break;
            case StageTileInfo.TYPE.TRAVEL:
                    ActionRemined = 0;
                    Travel();
                    break;
            case StageTileInfo.TYPE.START:
                    bool isAbleToBuild = false;
                    foreach (StageTileInfo t in lands.Values)
                    {
                        if (t.type == StageTileInfo.TYPE.CITY && t.buildings[4].isBuilt == false)
                        {
                            isAbleToBuild = true;
                            break;
                        }
                    }
                    if (isAbleToBuild)
                    {
                        BuyLandGameMessage receivedMessage = (BuyLandGameMessage)ReceivedMessage;
                        if (receivedMessage.Buildings > (char)0)
                        {
                            if (!BuyLand(receivedMessage))
                            {
                                SelfBan();
                            }
                        }
                    }
				break;
			}
		}
		
		private IEnumerable<IGameMessage> BuyLandRequest(StageTileInfo stageTile){
			yield return new BuyLandRequestGameMessage
            {
                Actor = Id,
                ResponseLimit = 5000
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
		
		private IEnumerable<IGameMessage> PayResultCitySight(StageTileInfo stageTile, int fee)
		{
			yield return new PayFeeGameMessage
            {
                Actor = Id,
				Fee = fee
            };
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
                        foreach (var gameAction in BuyLandRequest(stageTile)) yield return gameAction;
                    }
                }
                else
                {
                    SelfBan();
                }
            }
		}
		
        private IEnumerable<IGameMessage> MoveResultCitySight(StageTileInfo stageTile)
	    {
	        if (stageTile.IsAbleToBuy(this))
	        {
				foreach (var gameAction in BuyLandRequest(stageTile)) yield return gameAction;
	        }
	        else
	        {
	            if (stageTile.owner != null && !stageTile.IsSameOwner(this))
	            {
					int discount = 0;
					if ( chanceCoupon == CHANCE_COUPON.DISCOUNT_50 || chanceCoupon == CHANCE_COUPON.ANGEL ){
						yield return new UseCouponRequestGameMessage
						{
							Actor = Id
						};
						UseCouponGameMessage receivedMessage = (UseCouponGameMessage) ReceivedMessage;
						if ( receivedMessage.Use ){
							if ( chanceCoupon == CHANCE_COUPON.ANGEL ){
								discount = 100;
							}else if ( chanceCoupon == CHANCE_COUPON.DISCOUNT_50 ){
								discount = 50;
							}
							chanceCoupon = CHANCE_COUPON.NONE;
						}
					}
					int fee = GetPayFee(stageTile, discount);
					
					if (Pay (stageTile, discount)){
						foreach (var gameAction in PayResultCitySight(stageTile, fee)) yield return gameAction;
	                }else{
						if ( usableLoanCount > 0 ){
							long needMoney = fee - gold;
							yield return new NeedMoneyRequestGameMessage
							{
								Actor = Id,
								NeedMoney = needMoney
							};
							IDragonMarbleGameMessage receivedMessage = (LoanMoneyGameMessage) ReceivedMessage;
							if ( receivedMessage.MessageType == GameMessageType.LoanMoney ){
								LoanMoneyGameMessage loanMsg = (LoanMoneyGameMessage) receivedMessage;
								yield return loanMsg;
								
								if ( loanMsg.LoanMoney > 0 ){
									if ( Loan( needMoney ) ){
										if ( Pay(stageTile, discount) ){
											foreach (var gameAction in BuyLandRequest(stageTile)) yield return gameAction;
										}else{
											SelfBan();
										}
									}else{
										SelfBan();
									}
								}else{
									yield return new GameResultGameMessage
									{
										LoseUnit = Id
									};
								}
								
							}else if ( receivedMessage.MessageType == GameMessageType.SellLands ){
								SellLandsGameMessage sellMsg = (SellLandsGameMessage) receivedMessage;
								yield return sellMsg;
								foreach ( char c in sellMsg.LandList ){
									lands[(int) c].Sell();
								}
								if ( Pay(stageTile, discount) ){
									foreach (var gameAction in PayResultCitySight(stageTile, fee)) yield return gameAction;
								}else{
									SelfBan();
								}
							}

						}else{
							yield return new GameResultGameMessage
							{
								LoseUnit = Id
							};
						}

					}
	               
	            }
	        }
	        yield return null;
	    }
		
		private bool BuyLand(IDragonMarbleGameMessage receivedMessage){
			BuyLandGameMessage msg = (BuyLandGameMessage) receivedMessage;
            if (Stage.Tiles[msg.TileIndex].IsSameOwner(this) == false)
            {
                return false;
            }
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
		
		private void SelfBan()
		{
		    StageManager.Ban(this);
		}
	}
}