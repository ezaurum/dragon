using System;
using System.Collections.Generic;

namespace DragonMarble
{
    public class StageTile
    {   

        private readonly StageTileInfo _info;
        public int GroupId { get; set; }

        public bool FeeBoosted
        {
            get {
                return _info.isFestival;
            }
            set
            {
                _info.isFestival = value;
            }
        }

        public Int16 Position
        {
            get
            {
                return (short) _info.index;
            }
            set
            {
                _info.index = value;
            }
        }

        public StageTileInfo.TYPE Type
        {
            get
            {
                return _info.type;
            }
            set
            {
                _info.type = value;
            }
        }

        public StageTile(int index, string name, string type, string typeValue,
            int[] buyPrices, int[] sellPrices, int[] fees)
        {
            StageTileInfo.TYPE tileType = (StageTileInfo.TYPE)Enum.Parse(typeof(StageTileInfo.TYPE), type);
            _info = new StageTileInfo(index, name, tileType)
            {
                buildings = new List<StageTileInfo.Building>()
            };
            
            for (int i =0; i < buyPrices.Length ; i++)
            {
                _info.buildings.Add(new StageTileInfo.Building()
                {
                    buyPrice = buyPrices[i],
                    fee = fees[i],
                    sellPrice = sellPrices[i]
                });
            }

            GroupId = int.Parse(typeValue);
        }
    }
}