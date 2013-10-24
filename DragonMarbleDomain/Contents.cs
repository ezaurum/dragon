using System;
using System.Collections;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public static class RandomFactory
    {
        public static Random NewRandom()
        {
            return new Random();
        }
    }
	public static class RandomUtil
	{
		public static float Next(float min, float max){
            return RandomFactory.NewRandom().Next((int)(min * 1000), (int)max * 1000) / 1000f;
		}
	}
	

    public class StageBuffInfo
    {
        public enum TARGET
        {
            OWNER,
            CITY_ENEMY,
            CITYGROUP_ENEMY,
            BUILDING_ENEMY,
            BUILDINGGROUP_ENEMY
        }

        public enum TYPE
        {
            OVERCHARGE,
            DISCOUNT,
            DESTROY
        }

        public int power;
        public int turn;
        public TYPE type;

        public StageBuffInfo(TYPE type, int power, int turn)
        {
            this.type = type;
            this.turn = turn;
            this.power = power;
        }

        public bool isAlive
        {
            get
            {
                if (turn >= 0)
                {
                    return true;
                }
                return false;
            }
        }

        public void UpdateTurn()
        {
            turn--;
        }
    }

    public class StageDiceInfo
    {
        public enum ROLL_TYPE
        {
            NORMAL,
            ODD,
            EVEN
        }

        private readonly Random rand;
        public bool isDouble;

        public int[] result;
        public int rollCount;
        public ROLL_TYPE rollType;

        public StageDiceInfo()
        {
            rand = RandomFactory.NewRandom();
            result = new[] { 0, 0 };
            rollCount = 0;
            isDouble = false;
            rollType = ROLL_TYPE.NORMAL;
        }

        public int resultSum
        {
            get
            {
                int s = 0;
                foreach (int r in result)
                {
                    s += r;
                }
                return s;
            }
        }

        public void Roll()
        {
            result[0] = rand.Next(1, 7);
            result[1] = rand.Next(1, 7);

            int sum = result[0] + result[1];
            if ((rollType == ROLL_TYPE.ODD && sum % 2 == 0) || (rollType == ROLL_TYPE.EVEN && sum % 2 == 1))
            {
                if (result[0] == 1)
                {
                    result[0]++;
                }
                else if (result[0] == 6)
                {
                    result[0]--;
                }
                else
                {
                    int[] r = { 1, -1 };
                    result[0] += r[rand.Next(2)];
                }
            }


            if (result[0] == result[1])
            {
                isDouble = true;
            }
            else
            {
                isDouble = false;
            }
            rollType = ROLL_TYPE.NORMAL;
            rollCount++;
        }

        public void Clear()
        {
            rollCount = 0;
            isDouble = false;
            result[0] = 0;
            result[1] = 0;
        }

        public GameAction RollAndGetResultGameAction(StageUnitInfo actor, float pressed, bool odd, bool even)
        {
            Roll();
            return DiceGameAction(actor);
        }

        private GameAction DiceGameAction(StageUnitInfo stageUnitInfo)
        {
            return new GameAction()
            {
                Actor = stageUnitInfo,
                NeedOther = false,
                Type = GameMessageType.RollMoveDiceResult,
                Message = new RollMoveDiceResultGameMessage
                {
                    Actor = stageUnitInfo.Id,
                    Dices = new List<char> { (char)result[0], (char)result[1] },
                    RollCount = (char)rollCount,
                }
            };
        }
    }


    public interface IStageManager
    {
        Guid Id { get; set; }
    }
    
    public class StageTileInfo
    {
        public enum TYPE
        {
            START,
            CITY,
            GAMBLE,
            CHANCE,
            PRISON,
            SIGHT,
            OLYMPIC,
            TRAVEL,
            TAX
        }

        public int GroupId { get; set; }

        public bool FeeBoosted
        {
            get
            {
                return isFestival;
            }
            set
            {
                isFestival = value;
            }
        }

        public Int16 Position
        {
            get
            {
                return (short)index;
            }
            set
            {
                index = value;
            }
        }

        public TYPE Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public StageTileInfo(int index, string name, string type, string typeValue,
            int[] buyPrices, int[] sellPrices, int[] fees) : this(index,name, TYPE.CHANCE)
        {
            Type = (TYPE)Enum.Parse(typeof(TYPE), type);
            buildings = new List<Building>();            

            for (int i = 0; i < buyPrices.Length; i++)
            {
                buildings.Add(new Building()
                {
                    buyPrice = buyPrices[i],
                    fee = fees[i],
                    sellPrice = sellPrices[i]
                });
            }

            GroupId = int.Parse(typeValue);
        }
		
		public static readonly char[] BUILDING = {
			(char)1,
			(char)2,
			(char)4,
			(char)8,
			(char)16
		};
        public List<Building> buildings;

        public int index;

        public bool isFestival;
        public bool isOlympicCity;
        public string name;

        public StageUnitInfo owner;
        public StageBuffInfo tileBuff;
        public TYPE type;
        public int typeValue;

        public StageTileInfo(int index, string name, TYPE tileType)
        {
            this.index = index;
            type = tileType;
            this.name = name;

            tileBuff = null;

            isFestival = false;
            isOlympicCity = false;
        }

        public StageTileInfo(Hashtable data)
        {
            index = (int)data["Index"];
            type = (TYPE)Enum.Parse(typeof(TYPE), (string)data["Type"]);
            name = (string)data["Name"];
            typeValue = (int)data["TypeValue"];
            tileBuff = null;


            buildings = new List<Building>();
            if (type == TYPE.CITY || type == TYPE.SIGHT)
            {
                var priceData = (List<Hashtable>)data["Price"];
                foreach (Hashtable d in priceData)
                {
                    var building = new Building();
                    building.buyPrice = (int)d["BuyPrice"];
                    building.sellPrice = (int)d["SellPrice"];
                    building.fee = (int)d["Fee"];
                    building.isBuilt = false;

                    buildings.Add(building);
                }
            }
        }

        public int fee
        {
            get
            {
                int p = 0;
                foreach (Building b in buildings)
                {
                    if (b.isBuilt)
                    {
                        p += b.fee;
                    }
                }
                if (tileBuff != null)
                {
                    if (tileBuff.type == StageBuffInfo.TYPE.DISCOUNT)
                    {
                        p += (p * tileBuff.power / 100);
                    }
                }
                return p;
            }
        }
        public int minBuyPrice
        {
            get
            {
                foreach (Building b in buildings)
                {
                    if (!b.isBuilt)
                    {
                        return b.buyPrice;
                    }
                }
                return 0;
            }
        }


        public int builtPrice
        {
            get
            {
                int p = 0;
                foreach (Building b in buildings)
                {
                    if (b.isBuilt)
                    {
                        p += b.buyPrice;
                    }
                }
                return p;
            }
        }

        public int sellPrice
        {
            get
            {
                int p = 0;
                foreach (Building b in buildings)
                {
                    if (b.isBuilt)
                    {
                        p += b.sellPrice;
                    }
                }
                return p;
            }
        }
		
		public bool isAbleToBuildLandmark {
			get {
				if ( type == StageTileInfo.TYPE.CITY ){
					for ( int i = 1; i <= 3; i++ ){
						if ( !buildings[i].isBuilt ){
							return false;
						}
					}
					if ( !buildings[4].isBuilt ){
						return true;
					}
				}
				return false;
			}
		}

        public int takeOverPrice
        {
            get { return builtPrice * 2; }
        }

        public void AddBuff(StageBuffInfo.TYPE buffType, int power, int buffTurn)
        {
            if (buffType == StageBuffInfo.TYPE.DESTROY)
            {
                for (int i = 3; i >= 1; i--)
                {
                    if (buildings[i].isBuilt)
                    {
                        buildings[i].isBuilt = false;
                        break;
                    }
                }
            }

            if (buffTurn > 0)
            {
                tileBuff = new StageBuffInfo(buffType, power, buffTurn);
            }
        }

        public bool IsSameOwner(StageUnitInfo unit)
        {
            if (owner == null || !owner.Equals(unit))
            {
                return false;
            }
            return true;
        }


        public bool Buy(StageUnitInfo unit, List<int> buildingIndex)
        {
            int price = 0;
            foreach (int i in buildingIndex)
            {
				if ( i >= 4 ) return false;
				if ( buildings[i].isBuilt )	return false;
                price += buildings[i].buyPrice;
            }

            if (unit.AddGold(-price))
            {
                foreach (int i in buildingIndex)
                {
                    buildings[i].isBuilt = true;
                }
                if (owner == null)
                {
                    owner = unit;
                    unit.lands.Add(index, this);
                }
                return true;
            }
            return false;
        }

        public bool BuyLandmark(StageUnitInfo unit)
        {
            if ( isAbleToBuildLandmark )
            {
                if (unit.AddGold(-buildings[4].buyPrice))
                {
                    buildings[4].isBuilt = true;
                    return true;
                }
            }
            return false;
        }
		public bool IsAbleToBuy(StageUnitInfo unit){
			if ( type == TYPE.SIGHT || type == TYPE.CITY ){
				if ( owner == null || owner.Equals( unit ) ){
					if ( minBuyPrice > 0 && minBuyPrice <= unit.gold ){
						return true;
					}
				}
			}
			return false;
		}
		
        public bool TakeOver(StageUnitInfo unit)
        {
            int p = takeOverPrice;
            if (unit.AddGold(-p))
            {
                owner.AddGold(p);
                owner.lands.Remove(index);
                unit.lands.Add(index, this);
                owner = unit;
                return true;
            }
            return false;
        }

        public void SetOwner(StageUnitInfo unit)
        {
            owner = unit;
        }

        public void ChangeOwner(StageTileInfo tile)
        {
            StageUnitInfo exOwner = owner;
            owner = tile.owner;
            tile.owner = exOwner;
        }

        public void Sell()
        {
            owner.AddGold(sellPrice);
            owner.lands.Remove(index);
            owner = null;
            foreach (Building b in buildings) b.isBuilt = false;
        }

        public void Earthquake()
        {
            if (!buildings[4].isBuilt)
            {
                for (int i = 1; i <= 3; i++)
                {
                    buildings[i].isBuilt = false;
                }
            }
        }

        public void UpdateTurn()
        {
            if (tileBuff != null)
            {
                tileBuff.UpdateTurn();
                if (!tileBuff.isAlive)
                {
                    tileBuff = null;
                }
            }
        }

        public class Building
        {
            public int buyPrice;
            public int fee;
            public bool isBuilt;
            public int sellPrice;
        }
    }

    public class StageGambleInfo
    {
        public const int MAX_WIN_COUNT = 3;
        public const int BASIC_SHOW_COUNT = 4;
        public const int COMPARER_NUMBER = 7;
        public enum TYPE
        {
            spade, dia, heart, clover
        }
        public enum CHOICE
        {
            NULL, HIGH, LOW
        }
        public enum RESULT
        {
            NULL, WIN, LOSE
        }

        public class CardData
        {
            public TYPE type;
            public int num;
            public CardData(TYPE type, int num)
            {
                this.type = type;
                this.num = num;
            }
        }
        Random rand;
        public int winCount;
        public int rewardScale;
        public int battingPrice;
        public int rewardPrice;
        public RESULT result;
        public CHOICE selectChoice;
        public List<CardData> cards;
        public List<CardData> useCards;

        public StageGambleInfo()
        {
            rand = RandomFactory.NewRandom();
            winCount = 0;
            battingPrice = 0;
            rewardPrice = 0;
            result = RESULT.NULL;
            selectChoice = CHOICE.NULL;
        }

        public void SetBattingPrice(int price)
        {
            if (winCount == 0)
            {
                battingPrice = price;
                rewardScale = 2;
            }
        }


        public void InitCards()
        {
            cards = new List<CardData>();
            foreach (TYPE t in Enum.GetValues(typeof(TYPE)))
            {
                for (int i = 1; i <= 13; i++)
                {
                    cards.Add(new CardData(t, i));
                }
            }
            useCards = new List<CardData>();
        }

        public void UseBasicCards()
        {
            for (int i = 0; i < BASIC_SHOW_COUNT; i++)
            {
                PickOneCard();
            }
        }

        CardData PickOneCard()
        {
            int r = rand.Next(0, cards.Count);
            useCards.Add(cards[r]);
            cards.RemoveAt(r);
            return useCards[useCards.Count - 1];
        }

        public CardData SelectChoice(CHOICE c)
        {
            selectChoice = c;
            CardData card = PickOneCard();
            if ((c == CHOICE.HIGH && card.num >= COMPARER_NUMBER) || (c == CHOICE.LOW && card.num <= COMPARER_NUMBER))
            {
                result = RESULT.WIN;
                rewardPrice = battingPrice * rewardScale;
                winCount++;
                if (winCount < MAX_WIN_COUNT)
                {
                    rewardScale = rewardScale * 2;
                }

            }
            else
            {
                result = RESULT.LOSE;
                rewardPrice = 0;
            }
            return card;
        }

    }
}