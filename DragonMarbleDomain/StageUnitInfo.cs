using System;
using System.Collections.Generic;
using Dragon.Interfaces;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class StageUnitInfo
    {

        public StageUnitInfo(UNIT_COLOR unitColor, int initialCapital = 2000000)
            : this()
        {
            Id = Guid.NewGuid();
            this.unitColor = unitColor;
            gold = initialCapital;
        }

        public StageUnitInfo()
        {
            CharacterId = 1;
            round = 1;
            tileIndex = 0;
            lands = new Dictionary<int, StageTileInfo>();
            unitBuff = null;
            chanceCoupon = CHANCE_COUPON.NONE;
            usableLoanCount = 1;
			isBankrupt = false;
            DiceId = 1;
            Dice = new StageDiceInfo();
        }

        public Guid Id { get; set; }
        public CHANCE_COUPON chanceCoupon;
        public int gold;
        public Dictionary<int, StageTileInfo> lands;
        public int ranking;
        public int round;
        public TEAM_GROUP teamGroup;
        public int tileIndex;
        public StageBuffInfo unitBuff;
        public UNIT_COLOR unitColor;
		public SPECIAL_STATE specialState;
		public int specialStateValue;
        public int usableLoanCount;
		public bool isBankrupt;
        public virtual IMessageProcessor<IDragonMarbleGameMessage> MessageProcessor { get; set; }
        public virtual IStageManager StageManager { get; set; }
        public StageDiceInfo Dice { get; set; }
        public int Order { get; set; }
        public int ActionRemined { get; set; }
        public bool OwnTurn { get; set; }
        public ControlModeType ControlMode { get; set; }
        public int CharacterId { get; set; }
        public int DiceId { get; set; }

        public virtual IDragonMarbleGameMessage ReceivedMessage
        {
            get { return MessageProcessor.ReceivedMessage; }
			internal set { MessageProcessor.ReceivedMessage = value; }
        }

        public virtual IDragonMarbleGameMessage SendingMessage
        {
            set { MessageProcessor.SendingMessage = value; }
        }

        public int Assets
        {
            get { return property; }
        }

        public int LastSelected { get; set; }

        public int Position
        {
            get { return tileIndex; }
            set { tileIndex = value; }
        }

        public int Gold
        {
            get { return gold; }
            set { gold = value; }
        }

        public UNIT_COLOR UnitColor
        {
            get { return unitColor; }
            set { unitColor = value; }
        }

        public int property
        {
            get
            {
                int p = gold;
                foreach (StageTileInfo t in lands.Values)
                {
                    p += t.sellPrice;
                }
                return p;
            }
        }

        public bool isAbleToLoan
        {
            get
            {
                if (usableLoanCount > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public GameBoard Stage { get; set; }

        public void ResetMessages()
        {
            MessageProcessor.ResetMessages();
        }

        public bool AddGold(int a)
        {
            if (gold + a < 0) return false;
            gold += a;
            return true;
        }

        public int DonateMoney(int g)
        {
            if (g > gold) g = gold;
            AddGold(-g);
            return g;
        }

        public void UpdateTurn()
        {
            if (unitBuff != null)
            {
                unitBuff.UpdateTurn();
                if (!unitBuff.isAlive)
                {
                    unitBuff = null;
                }
            }
        }

        public void Go(int step)
        {
            tileIndex += step;
            if (tileIndex >= 32) tileIndex -= 32;
        }
		public void Prison(){
			specialState = StageUnitInfo.SPECIAL_STATE.PRISON;
			specialStateValue = 0;
		}
		public void Travel(){
			specialState = StageUnitInfo.SPECIAL_STATE.TRAVEL;
			specialStateValue = 0;
		}
		
		public void UpdatePrisonState(){
			if ( specialState == StageUnitInfo.SPECIAL_STATE.PRISON ){
				specialStateValue++;
				if ( specialStateValue >= 3 ){
					specialState = SPECIAL_STATE.NONE;
					specialStateValue = 0;
				}
			}
		}
		
        public bool Loan(int loanGold)
        {
            if (usableLoanCount > 0)
            {
                usableLoanCount--;
                AddGold(loanGold);
                return true;
            }
            return false;
        }
		
		public int GetPayFee(StageTileInfo tile){
			int fee = tile.fee;
            if (unitBuff != null)
            {
                if (unitBuff.type == StageBuffInfo.TYPE.OVERCHARGE)
                {
                    fee += (fee*unitBuff.power/100);
                }
                else if (unitBuff.type == StageBuffInfo.TYPE.DISCOUNT)
                {
                    fee -= (fee*unitBuff.power/100);
                }
            }
			return fee;
		}
        public bool Pay(StageTileInfo tile)
        {
			int fee = GetPayFee(tile);
            if (AddGold(-fee))
            {
                tile.owner.AddGold(fee);
                return true;
            }
            return false;
        }

        public int GetTax(int taxPercent)
        {
            int p = 0;
            foreach (StageTileInfo t in lands.Values)
            {
                p += t.builtPrice;
            }
            return p*taxPercent/100;
        }

        public void AddBuff(StageBuffInfo.TYPE buffType, int buffTurn, int power)
        {
            if (buffTurn > 0)
            {
                unitBuff = new StageBuffInfo(buffType, buffTurn, power);
            }
        }
    }
}