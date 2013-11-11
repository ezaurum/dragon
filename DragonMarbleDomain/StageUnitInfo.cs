using System;
using System.Collections.Generic;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    [Serializable]
    public partial class StageUnitInfo
    {
        private bool _isReady;
        public CHANCE_COUPON chanceCoupon;
        public long gold;
        public bool isBankrupt;
        public Dictionary<int, StageTileInfo> lands;
        public int ranking;
        public int round;
        public SPECIAL_STATE specialState;
        public int specialStateValue;
        public TEAM_GROUP teamGroup;
        public int tileIndex;
        public StageBuffInfo unitBuff;
        public UNIT_COLOR unitColor;
        public int usableLoanCount;

        public StageUnitInfo(UNIT_COLOR unitColor, TEAM_GROUP teamGroup, int initialCapital = 2000000)
            : this(unitColor, initialCapital)
        {
            this.teamGroup = teamGroup;
        }

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
            specialState = SPECIAL_STATE.NONE;
        }

        public Guid Id { get; set; }
        public virtual IMessageProcessor<IDragonMarbleGameMessage> MessageProcessor { get; set; }
        public virtual IStageManager StageManager { get; set; }
        public StageDiceInfo Dice { get; set; }
        public int Order { get; set; }
        public int ActionRemined { get; set; }
        public bool OwnTurn { get; set; }
        public ControlModeType ControlMode { get; set; }
        public int CharacterId { get; set; }
        public int DiceId { get; set; }

        public bool IsReady
        {
            get { return IsRoomOwner || _isReady; }
            set
            {
                _isReady = value;
            }
        }

        public bool IsRoomOwner { get; set; }

        public virtual IDragonMarbleGameMessage ReceivedMessage
        {
            get { return MessageProcessor.ReceivedMessage; }
            set
            {
                if (!OwnTurn)
                {
                    foreach (var messages in GetMessageResult(value))
                    {
                        StageManager.Notify(messages);
                    }
                }
            }
        }

        public virtual IDragonMarbleGameMessage SendingMessage
        {
            set { MessageProcessor.SendingMessage = value; }
        }

        public long Assets
        {
            get { return property; }
        }

        public int Position
        {
            get { return tileIndex; }
            set { tileIndex = value; }
        }

        public long Gold
        {
            get { return gold; }
            set { gold = value; }
        }

        public UNIT_COLOR UnitColor
        {
            get { return unitColor; }
            set { unitColor = value; }
        }

        public long property
        {
            get
            {
                long p = gold;
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

        public bool AddGold(long a)
        {
            if (gold + a < 0) return false;
            gold += a;
            return true;
        }

        public long DonateMoney(long g)
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
            if (tileIndex >= 32){
				tileIndex -= 32;
				round++;
				AddGold( GameBoard.SALARY );
			}
        }
		public void GoTo(int goTileIndex){
			int goStep = goTileIndex - tileIndex;
			if ( goStep < 0 ) goStep += 32;
			Go(goStep);
		}
		

        public void Prison()
        {
            specialState = SPECIAL_STATE.PRISON;
            specialStateValue = 0;
        }

        public void Travel()
        {
            specialState = SPECIAL_STATE.TRAVEL;
            specialStateValue = 0;
        }

        public void UpdatePrisonState()
        {
            if (specialState == SPECIAL_STATE.PRISON)
            {
                specialStateValue++;
                if (specialStateValue >= 3)
                {
                    specialState = SPECIAL_STATE.NONE;
                    specialStateValue = 0;
                }
            }
        }

        public bool Loan(long loanGold)
        {
            if (usableLoanCount > 0)
            {
                usableLoanCount--;
                AddGold(loanGold);
                return true;
            }
            return false;
        }

        public long GetPayFee(StageTileInfo tile)
        {
            long fee = tile.fee;
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
            //fee -= (fee*discount/100);
            return fee;
        }

        public bool Pay(StageTileInfo tile)
        {
            long fee = GetPayFee(tile);
            if (AddGold(-fee))
            {
                tile.owner.AddGold(fee);
                return true;
            }
            return false;
        }

        public long GetTax()
        {
            long p = 0;
            foreach (StageTileInfo t in lands.Values)
            {
                p += t.builtPrice;
            }
            return p*GameBoard.TAX_PERCENT/100;
        }
		public bool PayTax(){
			return AddGold( - GetTax() );
		}
		
        public void AddBuff(StageBuffInfo.TYPE buffType, int buffTurn, int power)
        {
            if (buffTurn > 0)
            {
                unitBuff = new StageBuffInfo(buffType, buffTurn, power);
            }
        }

        public void SelectOrderCard(IDragonMarbleGameMessage value)
        {
            StageManager.OrderSelectSended((OrderCardSelectGameMessage) value);
        }
    }
}