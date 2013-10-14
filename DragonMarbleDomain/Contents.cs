using System;
using System.Collections;
using System.Collections.Generic;

namespace DragonMarble
{
    public class StageBuffInfo
    {

        public enum TYPE
        {
            OVERCHARGE, DISCOUNT, DESTROY
        }
        public enum TARGET
        {
            OWNER, CITY_ENEMY, CITYGROUP_ENEMY, BUILDING_ENEMY, BUILDINGGROUP_ENEMY
        }

        public int turn;
        public int power;
        public TYPE type;

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
    
        public StageBuffInfo(TYPE type, int power, int turn)
        {
            this.type = type;
            this.turn = turn;
            this.power = power;
        }
        public void UpdateTurn()
        {
            turn--;
        }
    }

    public class StageDiceInfo
    {
        Random rand;

        public enum ROLL_TYPE
        {
            NORMAL, ODD, EVEN
        }
        public ROLL_TYPE rollType;

        public int[] result;
        public int rollCount;
        public bool isDouble;
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

        public StageDiceInfo()
        {
            rand = new Random();
            result = new int[] { 0, 0 };
            rollCount = 0;
            isDouble = false;
            rollType = ROLL_TYPE.NORMAL;
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

        public enum TEAM_COLOR
        {
            RED = 0, BLUE, YELLOW, GREEN, PINK, SKY
        }
        public enum CHANCE_COUPON
        {
            NULL, DISCOUNT_50, ESCAPE_ISLAND, SHIELD, ANGEL
        }
        public TEAM_COLOR teamColor;
        public int gold;
        public Dictionary<int, StageTileInfo> lands;
        public int tileIndex;
        public int ranking;
        public int round;
        public StageBuffInfo unitBuff;
        public CHANCE_COUPON chanceCoupon;
        public int Order { get; set; }
        public int CapitalOrder { get; set; }
        public int Capital { get; set; }
        public bool OwnTurn { get; set; }

        public StageUnitInfo(TEAM_COLOR teamColor, int initialCapital = 2000000)
        {
            this.teamColor = teamColor;
            round = 1;
            tileIndex = 0;
            lands = new Dictionary<int, StageTileInfo>();
            unitBuff = null;
            Capital = initialCapital;
            gold = initialCapital;
            chanceCoupon = CHANCE_COUPON.NULL;
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

    }

    public class StageTileInfo
    {

        public enum TYPE
        {
            START, CITY, GAMBLE, CHANCE, PRISON, SIGHT, OLYMPIC, TRAVEL, TAX
        }
        public int index;
        public TYPE type;
        public string name;
        public int typeValue;

        public bool isFestival;
        public bool isOlympicCity;

        public class Building
        {
            public int buyPrice;
            public int sellPrice;
            public int fee;
            public bool isBuilt;
        }
        public List<Building> buildings;
        public StageBuffInfo tileBuff;
        public StageUnitInfo owner;

        public StageTileInfo(int index, string name, TYPE tileType)
        {
            this.index = index;
            this.type = tileType;
            this.name = name;

            tileBuff = null;

            isFestival = false;
            isOlympicCity = false;
        }

        public StageTileInfo(Hashtable data)
        {
            index = (int)data["Index"];
            type = (StageTileInfo.TYPE)Enum.Parse(typeof(StageTileInfo.TYPE), (string)data["Type"]);
            name = (string)data["Name"];
            typeValue = (int)data["TypeValue"];
            tileBuff = null;


            buildings = new List<Building>();
            if (type == TYPE.CITY || type == TYPE.SIGHT)
            {
                List<Hashtable> priceData = (List<Hashtable>)data["Price"];
                foreach (Hashtable d in priceData)
                {
                    Building building = new Building();
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
            get
            {
                return builtPrice * 2;
            }
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
            else
            {
                return false;
            }

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


    }


}