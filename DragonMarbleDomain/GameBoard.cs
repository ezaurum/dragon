﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonMarble
{
    public class GameBoard
    {   
       	//public const int IndexOfPrison = 8;
		
		public const int SALARY = 200000;
		public const int PRISON_PRICE = 300000;
		public const int DONATE_MONEY = 200000;
		public const int TAX_PERCENT = 5;
		
		public int TILE_INDEX_PRISON;
		public int TILE_INDEX_TRAVEL;
		public int TILE_INDEX_IOC;
		public int tile_index_olympic;
		
        public int GrossAssets { get; set; }
        public List<StageTileInfo> Tiles { get; set; }
        public Dictionary<StageTileInfo.ColorGroupType, List<StageTileInfo>> ColorGroups { get; set; }
        public Dictionary<StageTileInfo.LineGroupType, List<StageTileInfo>> LineGroups { get; set; }
        //public Dictionary<> 

        public GameBoard(List<StageTileInfo> tiles)
        {
            Tiles = tiles;
            FeeBoostedTiles = new List<short>();
        }

        public void Init()
        {
            //StageTile from _tiles
            IEnumerable<StageTileInfo> citiesAndSights 
                = Tiles.Where(t => StageTileInfo.TYPE.CITY == t.Type 
                    || StageTileInfo.TYPE.SIGHT == t.Type);

            List<StageTileInfo> stageTileInfos = citiesAndSights.ToList();

            if (stageTileInfos.Count < 4)
            {
                throw new ArgumentOutOfRangeException(string.Format("not enough tiles. {0}", stageTileInfos.Count));
            }

            //line groups list added
            LineGroups = new Dictionary<StageTileInfo.LineGroupType, List<StageTileInfo>>();
            foreach (StageTileInfo.LineGroupType key in Enum.GetValues(typeof(StageTileInfo.LineGroupType)))
            {
                LineGroups.Add(key, new List<StageTileInfo>());
            }
            
            //color groups list added
            ColorGroups = new Dictionary<StageTileInfo.ColorGroupType, List<StageTileInfo>>();
            foreach (StageTileInfo.ColorGroupType key in Enum.GetValues(typeof(StageTileInfo.ColorGroupType)))
            {
                ColorGroups.Add(key,new List<StageTileInfo>());
            }

            foreach (StageTileInfo stageTileInfo in Tiles)
            {
                if (StageTileInfo.TYPE.CITY != stageTileInfo.Type 
                    || StageTileInfo.TYPE.SIGHT != stageTileInfo.Type)
                    continue;

                //set color group for cities
                if (StageTileInfo.TYPE.CITY == stageTileInfo.Type)
                {
                    List<StageTileInfo> colorGroup = ColorGroups[stageTileInfo.Color];
                    colorGroup.Add(stageTileInfo);
                    stageTileInfo.colorGroup = colorGroup;
                }

                //set line group
                int a = 0;
                foreach (StageTileInfo.LineGroupType key in Enum.GetValues(typeof(StageTileInfo.LineGroupType)))
                {
                    int b = (int) key;
                    if (a < stageTileInfo.index && stageTileInfo.index < b)
                    {
                        stageTileInfo.Line = (StageTileInfo.LineGroupType) b;
                        List<StageTileInfo> tileInfos  = LineGroups[stageTileInfo.Line];
                        tileInfos.Add(stageTileInfo);
                        stageTileInfo.lineGroup = tileInfos;
                    }
                    a = b;
                }
            }

            Random r = new Random();
            while (FeeBoostedTiles.Count < 4)
            {
                int next = r.Next(0, stageTileInfos.Count());

                StageTileInfo targetTile = stageTileInfos[next];
                stageTileInfos.Remove(targetTile);
                
                if ( targetTile.FeeBoosted ) continue;
                
                targetTile.FeeBoosted = true;
                FeeBoostedTiles.Add(targetTile.Position);
            }
        }

        public List<short> FeeBoostedTiles {get; set;}
        
    }
}