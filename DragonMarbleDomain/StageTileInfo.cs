using System;
using System.Collections;
using System.Collections.Generic;

namespace DragonMarble
{
	[Serializable]
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

        public enum ColorGroupType
        {
            Olive = 1,
            Green = 2,
            Sky = 3,
            Blue = 4,
            Pink = 5,
            Violet = 6,
            Orange = 7,
            Red = 8
        }

        public enum LineGroupType
        {
            //Bottom is left line from start tile
            Bottom = 8,
            //value is tile index
            Left = 16, Top =24, Right =32,

        }
        
        public ColorGroupType Color { get; set; }
        public LineGroupType Line { get; set; }

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
            
            this.typeValue = int.Parse(typeValue);
        }
		
        public static readonly char[] BUILDING = {
            (char)1,
            (char)2,
            (char)4,
            (char)8,
            (char)16
        };
        public List<Building> buildings;
        public List<StageTileInfo> colorGroup;
        public List<StageTileInfo> lineGroup;
		
        public int index;

        public bool isFestival;
        public int olympic;
        public bool isMonopoly;
        public string name;
		
		private StageUnitInfo _owner;
		public StageUnitInfo owner {
			get {
				return _owner;
			}
			set {
				if ( _owner != null ){
					_owner.lands.Remove( index );
				}
				_owner = value;
				if ( _owner != null ){
					_owner.lands.Add( index, this );
				}
			}
		}
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
            olympic = 0;
            isMonopoly = false;
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

        public long fee
        {
            get
            {
                long p = 0;
                foreach (Building b in buildings)
                {
                    if (b.isBuilt)
                    {
                        p += b.fee;
                    }
                }
                if ( olympic > 0 ) p = p * olympic;
                if ( isFestival ) p = p * 2;
                if ( isMonopoly ) p = p * 2;
				
                if (tileBuff != null)
                {
                    if (tileBuff.type == StageBuffInfo.TYPE.DISCOUNT)
                    {
                        p -= (p * tileBuff.power / 100);
                    }
                }
                return p;
            }
        }
        public int GetMinBuyPrice(StageUnitInfo unit)
        {
			if ( isAbleToBuildLandmark ){
				return buildings[4].buyPrice;
			}
            for ( int i = 0; i < 4; i++ ){
				if ( i >= buildings.Count ) return 0;
				if ( buildings[i].isBuilt == false ){
					if ( i <= unit.round ){
						return buildings[i].buyPrice;
					}
					return 0;
				}
            }
            return 0;
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
		public bool IsEnemyTeam(StageUnitInfo unit){
			if ( owner == null || owner.teamGroup == unit.teamGroup ){
				return false;
			}
			return true;
		}
		
        public bool IsSameOwner(StageUnitInfo unit)
        {
            if (owner == null || !owner.Equals(unit))
            {
                return false;
            }
            return true;
        }

        public bool IsSameTeam(StageUnitInfo unit)
        {
            if (owner == null || owner.teamGroup != unit.teamGroup)
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
                if ( owner == null || owner.teamGroup == unit.teamGroup ){
                    int minBuyPrice = GetMinBuyPrice( unit );
                    if ( minBuyPrice > 0 && minBuyPrice <= unit.gold ){
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsAbleToTakeover(StageUnitInfo unit){
            if ( type == TYPE.CITY && owner != null && owner.Equals(unit) == false && buildings[4].isBuilt == false ){
                if ( unit.gold >= this.takeOverPrice ){
                    return true;
                }
            }
            return false;
        }
		
        public bool TakeOver(StageUnitInfo unit)
        {
			if ( owner.teamGroup == unit.teamGroup ) return false;
            int p = takeOverPrice;
            if (unit.AddGold(-p))
            {
                owner.AddGold(p);
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

        public bool UpdateTurn()
        {
            if (tileBuff != null)
            {
                tileBuff.UpdateTurn();
                if (!tileBuff.isAlive)
                {
                    tileBuff = null;
                }
				return true;
            }
			return false;
        }
		
		[Serializable]
        public class Building
        {
            public int buyPrice;
            public int fee;
            public bool isBuilt;
            public int sellPrice;
        }
    }
}