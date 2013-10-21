using System;
using System.Collections;
using System.Collections.Generic;
using Dragon.Interfaces;
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
    }

    public class StageUnitInfo
    {
        public virtual IMessageProcessor<IDragonMarbleGameMessage> MessageProcessor { get; set; }
        public virtual IStageManager StageManager { get; set; }
        public StageDiceInfo Dice { get; set; }

        public virtual IDragonMarbleGameMessage ReceivedMessage
        {
            get { return MessageProcessor.ReceivedMessage; }
        }

        public virtual IDragonMarbleGameMessage SendingMessage
        {
            set { MessageProcessor.SendingMessage = value; }
        }

        public enum ControlModeType
        {
            Player, AI_0, AI_1, AI_2,
        }

        public enum UNIT_COLOR
        {
            RED = 0, BLUE, YELLOW, GREEN, PINK, SKY
        }
        public enum TEAM_GROUP
        {
            A = 0, B, C, D
        }
        public enum CHANCE_COUPON
        {
            NULL, DISCOUNT_50, ESCAPE_ISLAND, SHIELD, ANGEL
        }
        public UNIT_COLOR unitColor;
        public TEAM_GROUP teamGroup;

        public int gold;
        public Dictionary<int, StageTileInfo> lands;
        public int tileIndex;
        public int ranking;
        public int round;
        public StageBuffInfo unitBuff;
        public CHANCE_COUPON chanceCoupon;
        public int Order { get; set; }
        public int CapitalOrder { get; set; }

        public int Capital
        {
            get
            {
                return property;
            }
        }

        public int ActionRemined { get; set; }


        public bool OwnTurn { get; set; }
        public int DiceId { get; set; }
        public ControlModeType ControlMode { get; set; }
        public int CharacterId { get; set; }
        public Guid Id { get; set; }

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
            chanceCoupon = CHANCE_COUPON.NULL;
            DiceId = 1;
            Dice = new StageDiceInfo();
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

        public bool Pay(StageTileInfo tile)
        {
            int fee = tile.fee;
            if (unitBuff != null)
            {
                if (unitBuff.type == StageBuffInfo.TYPE.OVERCHARGE)
                {
                    fee += (fee * unitBuff.power / 100);
                }
                else if (unitBuff.type == StageBuffInfo.TYPE.DISCOUNT)
                {
                    fee -= (fee * unitBuff.power / 100);
                }
            }

            if (AddGold(-fee))
            {
                tile.owner.AddGold(fee);
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetTax(int taxPercent)
        {
            int p = 0;
            foreach (StageTileInfo t in lands.Values)
            {
                p += t.builtPrice;
            }
            return p * taxPercent / 100;
        }

        public void AddBuff(StageBuffInfo.TYPE buffType, int buffTurn, int power)
        {
            if (buffTurn > 0)
            {
                unitBuff = new StageBuffInfo(buffType, buffTurn, power);
            }
        }

        public void DoAction(IDragonMarbleGameMessage message)
        {
            switch (message.MessageType)
            {
                case GameMessageType.RollMoveDice:
                    RollMoveDiceGameMessage rollMoveDiceGameMessage = (RollMoveDiceGameMessage)message;
                    //RollMoveDice(rollMoveDiceGameMessage.Pressed);
                    Console.WriteLine("{0}", Dice);
                    RollMoveDiceResultGameMessage
                    rmdrgm = new RollMoveDiceResultGameMessage()
                    {
                        From = StageManager.Id,
                        To = Id,
                        Dices = new List<char> { (char)Dice.result[0], (char)Dice.result[1] }
                    };
                    if (Dice.isDouble && Dice.rollCount < 3) ActionRemined += 1;
                    //SendingMessage = rmdrgm;
                    break;

                case GameMessageType.RollMoveDiceResult:
                    {
                        RollMoveDiceResultGameMessage rollMoveDiceResultGameMessage = (RollMoveDiceResultGameMessage)message;
                        int diceSum = 0;
                        foreach (char i in rollMoveDiceResultGameMessage.Dices) diceSum += i;
                        Go(diceSum);


                        break;
                    }


            }

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
            if (!buildings[4].isBuilt)
            {
                if (unit.AddGold(-buildings[4].buyPrice))
                {
                    buildings[4].isBuilt = true;
                    return true;
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