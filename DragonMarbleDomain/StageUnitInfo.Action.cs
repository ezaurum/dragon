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
			case StageTileInfo.TYPE.CHANCE:
				foreach (var gameAction in ChanceCardOpen()) yield return gameAction;
			        break;
			case StageTileInfo.TYPE.OLYMPIC:
				foreach (var gameAction in OpenOlympic()) yield return gameAction;
			        break;
				break;
			case StageTileInfo.TYPE.TAX:
				foreach (var gameAction in PayTaxResult()) yield return gameAction;
			        break;
				break;
			case StageTileInfo.TYPE.GAMBLE:
				foreach (var gameAction in GamblePlay()) yield return gameAction;
			        break;
				break;
				
				
				
			}
		}
		
		private IEnumerable<IGameMessage> GamblePlay(){
			if ( StageGambleInfo.BATTING_PRICE[0] > gold ){
				yield break;
			}
			
			StageGambleInfo gambleData = new StageGambleInfo();
			gambleData.InitCards();
			gambleData.UseBasicCards();
			
			List<char> cardList = new List<char>();
			for ( int i = 0; i < gambleData.useCards.Count; i++ ){
				cardList.Add( gambleData.MakeDataFromCard(gambleData.useCards[i]) );
			}
			yield return new GambleRequestGameMessage
			{
				Actor = Id,
				CardList = cardList
			};
			
			GambleBattingGameMessage battingMsg = (GambleBattingGameMessage) ReceivedMessage;
			if ( !AddGold( - StageGambleInfo.BATTING_PRICE[battingMsg.BattingIndex] ) ){
				SelfBan();
			}
			yield return battingMsg;
			
			foreach (var gameAction in GambleChoice(gambleData)) yield return gameAction;
			
		}
		
		private IEnumerable<IGameMessage> GambleChoice(StageGambleInfo gambleData){
			GambleChoiceGameMessage choiceMsg = (GambleChoiceGameMessage) ReceivedMessage;
			StageGambleInfo.CardData card = gambleData.SelectChoice( (StageGambleInfo.CHOICE) choiceMsg.Choice );
			
			switch( (StageGambleInfo.CHOICE) choiceMsg.Choice ){
			case StageGambleInfo.CHOICE.STOP:
				if ( gambleData.winCount > 0 ){
					AddGold( gambleData.rewardPrice );
					yield return new GambleResultGameMessage {
						Actor = Id,
						Choice = choiceMsg.Choice,
						WinCount = (char) gambleData.winCount,
						Card = gambleData.MakeDataFromCard(card)
					};
				}else{
					SelfBan();
				}
				break;
			case StageGambleInfo.CHOICE.HIGH:
			case StageGambleInfo.CHOICE.LOW:
				yield return new GambleResultGameMessage {
					Actor = Id,
					Choice = choiceMsg.Choice,
					WinCount = (char) gambleData.winCount,
					Card = gambleData.MakeDataFromCard(card)
				};
				if ( gambleData.winCount > 0 ){
					if ( gambleData.winCount == 3 ){
						AddGold( gambleData.rewardPrice );
						yield break;
					}else{
						foreach (var gameAction in GambleChoice(gambleData)) yield return gameAction;
					}
				}else{
					yield break;
				}
				break;
				
			}
		}
		
		private IEnumerable<IGameMessage> PayTaxResult(){
			long tax = GetTax();
			if ( PayTax() ){
				yield break;
			}else{
				if ( usableLoanCount > 0 ){
					long needMoney = tax - gold;
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
								if ( PayTax() ){
									yield break;
								}else{
									SelfBan();
								}
							}else{
								SelfBan();
							}
						}else{
							yield return new GameResultGameMessage
							{
								WinTeam = teamGroup
							};
						}
						
					}else if ( receivedMessage.MessageType == GameMessageType.SellLands ){
						SellLandsGameMessage sellMsg = (SellLandsGameMessage) receivedMessage;
						yield return sellMsg;
						foreach ( char c in sellMsg.LandList ){
							lands[(int) c].Sell();
						}
						if ( PayTax() ){
							yield break;
						}else{
							SelfBan();
						}
					}

				}else{
					yield return new GameResultGameMessage
					{
						WinTeam = teamGroup
					};
				}
			}
		}
		
		private IEnumerable<IGameMessage> OpenOlympic(){
			if ( lands.Count == 0 ) yield break;
			OpenOlympicGameMessage receivedMessage = (OpenOlympicGameMessage) ReceivedMessage;
			if ( receivedMessage.TileIndex > 0 ){
				if ( lands.ContainsKey(receivedMessage.TileIndex) ){
					Stage.OpenOlympicCity(receivedMessage.TileIndex);
				}else{
					SelfBan();
				}
			}
			yield return receivedMessage;
		}
		
		
		private IEnumerable<IGameMessage> ChanceCardOpen(){
			int[] chanceGroup = { 2,7,8,19,13,16,15,21,10 };
			int chanceId = chanceGroup[RandomUtil.Next(0, chanceGroup.Length)];
			//StageManager.Cards
			yield return new OpenChanceCardGameMessage
			{
				CardId = (char)chanceId,
				Actor = Id
			};
			IDragonMarbleGameMessage receivedMessage = ReceivedMessage;
			StageChanceCardInfo chance = StageManager.Cards[chanceId];
			switch( chance.type ){
			case StageChanceCardInfo.TYPE.GOTO:
				ChanceCardGoToGameMessage gotoMsg = (ChanceCardGoToGameMessage) receivedMessage;
				GoTo(chance.tileIndex);
				yield return receivedMessage;
				foreach (var destinationGameAction in DestinationGameAction())
	            {
	                yield return destinationGameAction;
	            }
				break;
			case StageChanceCardInfo.TYPE.BUFF:
				ChanceCardBuffGameMessage buffMsg = (ChanceCardBuffGameMessage) receivedMessage;
				switch( chance.buffTarget ){
				case StageBuffInfo.TARGET.OWNER:
					AddBuff(chance.buffType, chance.buffPower, chance.buffTurn);
					yield return receivedMessage;
					break;
				case StageBuffInfo.TARGET.BUILDING_ENEMY:
				case StageBuffInfo.TARGET.CITY_ENEMY:
				{
					StageTileInfo tile = Stage.Tiles[buffMsg.SelectTile];
					if ( tile.IsEnemyTeam(this) ){
						tile.AddBuff( chance.buffType, chance.buffPower, chance.buffTurn);
						yield return receivedMessage;
					}else{
						SelfBan();
					}
					break;
				}
				case StageBuffInfo.TARGET.BUILDINGGROUP_ENEMY:
				case StageBuffInfo.TARGET.CITYGROUP_ENEMY:
				{
					StageTileInfo tile = Stage.Tiles[buffMsg.SelectTile];
					if ( tile.IsEnemyTeam(this) ){
						tile.AddBuff(chance.buffType, chance.buffPower, chance.buffTurn);
						foreach ( StageTileInfo t in tile.colorGroup ){
							if ( t.owner != null && t.owner.teamGroup == tile.owner.teamGroup ){
								t.AddBuff( chance.buffType, chance.buffPower, chance.buffTurn);
							}
						}
						yield return receivedMessage;
					}else{
						SelfBan();
					}
					break;
				}
				}
				break;
			case StageChanceCardInfo.TYPE.ORDER:
				ChanceCardOrderGameMessage orderMsg = (ChanceCardOrderGameMessage) receivedMessage;
				switch( chance.orderType ){
				case StageChanceCardInfo.ORDER_TYPE.GO_ISLAND:
					tileIndex = Stage.TILE_INDEX_PRISON;
					Prison();
					yield return receivedMessage;
					break;
				case StageChanceCardInfo.ORDER_TYPE.GO_OLYMPIC_CITY:
					if ( Stage.tile_index_olympic >= 0 ){
						GoTo(Stage.tile_index_olympic);
						yield return receivedMessage;
						foreach (var destinationGameAction in DestinationGameAction())
			            {
			                yield return destinationGameAction;
			            }
						
						break;
					}else{
						yield return receivedMessage;
					}
					break;
				case StageChanceCardInfo.ORDER_TYPE.OPEN_OLYMPIC:
					yield return receivedMessage;
					foreach (var gameAction in OpenOlympic()) yield return gameAction;
					break;
				case StageChanceCardInfo.ORDER_TYPE.DONATE_CITY:
				{
					if ( lands.ContainsKey(orderMsg.Value1) && Id.Equals(orderMsg.Target) == false ){
						StageTileInfo tile = Stage.Tiles[orderMsg.Value1];
						tile.SetOwner(StageManager.Units[orderMsg.Target]);
						yield return receivedMessage;
					}else{
						SelfBan();
					}
					break;
				}
				case StageChanceCardInfo.ORDER_TYPE.DONATE_MONEY:
					DonateMoneyToPoorest();
					yield return receivedMessage;
					break;
				case StageChanceCardInfo.ORDER_TYPE.SELL:
					if ( lands.ContainsKey(orderMsg.Value1) ){
						lands[orderMsg.Value1].Sell();
						yield return receivedMessage;
					}else{
						SelfBan();
					}
					break;
				case StageChanceCardInfo.ORDER_TYPE.CHANGE_CITY:
					if ( lands.ContainsKey(orderMsg.Value1) && Stage.Tiles[orderMsg.Value2].IsEnemyTeam(this) ){
						lands[orderMsg.Value1].ChangeOwner(Stage.Tiles[orderMsg.Value2]);
						yield return receivedMessage;
					}else{
						SelfBan();
					}
					break;
				}
				break;
			case StageChanceCardInfo.TYPE.COUPON:
				ChanceCardCouponGameMessage couponMsg = (ChanceCardCouponGameMessage) receivedMessage;
				if ( couponMsg.Save ){
					chanceCoupon = chance.couponType;
				}
				yield return receivedMessage;
				break;
			}
		}
		
		private void DonateMoneyToPoorest(){
			//System.Guid poor = Id;
			StageUnitInfo poorUnit = this;
			foreach ( StageUnitInfo u in StageManager.Units.Values ){
				if ( !poorUnit.Equals(u) ){
					if ( poorUnit.property > u.property ){
						poorUnit = u;
					}
				}
			}
			long m = 0;
			foreach ( StageUnitInfo u in StageManager.Units.Values ){
				if ( !poorUnit.Equals(u) ){
					m += u.DonateMoney(GameBoard.DONATE_MONEY);
				}
			}
			poorUnit.AddGold(m);
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
		
		private IEnumerable<IGameMessage> PayResultCitySight(StageTileInfo stageTile, long fee)
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
					if ( chanceCoupon == CHANCE_COUPON.DISCOUNT_50 || chanceCoupon == CHANCE_COUPON.ANGEL ){
						yield return new UseCouponRequestGameMessage
						{
							Actor = Id
						};
						UseCouponGameMessage receivedMessage = (UseCouponGameMessage) ReceivedMessage;
						if ( receivedMessage.Use ){
							if ( chanceCoupon == CHANCE_COUPON.ANGEL ){
								AddBuff(StageBuffInfo.TYPE.DISCOUNT, 1, 100);
							}else if ( chanceCoupon == CHANCE_COUPON.DISCOUNT_50 ){
								AddBuff(StageBuffInfo.TYPE.DISCOUNT, 1, 50);
							}
							chanceCoupon = CHANCE_COUPON.NONE;
						}
					}
					long fee = GetPayFee(stageTile);
					
					if (Pay (stageTile)){
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
										if ( Pay(stageTile) ){
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
										WinTeam = teamGroup
									};
								}
								
							}else if ( receivedMessage.MessageType == GameMessageType.SellLands ){
								SellLandsGameMessage sellMsg = (SellLandsGameMessage) receivedMessage;
								yield return sellMsg;
								foreach ( char c in sellMsg.LandList ){
									lands[(int) c].Sell();
								}
								if ( Pay(stageTile) ){
									foreach (var gameAction in PayResultCitySight(stageTile, fee)) yield return gameAction;
								}else{
									SelfBan();
								}
							}

						}else{
							yield return new GameResultGameMessage
							{
								WinTeam = teamGroup
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