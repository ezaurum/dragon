﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonMarble
{
    public class GameBoard
    {
        private readonly List<StageTile> _tiles;
        public int GrossAssets { get; set; }

        public GameBoard(List<StageTile> tiles)
        {
            _tiles = tiles;
            FeeBoostedTiles = new List<short>();
        }

        public void Init()
        {
            //StageTile from _tiles
            IEnumerable<StageTile> citiesAndSights 
                = _tiles.Where(t => StageTileInfo.TYPE.CITY == t.Type 
                    || StageTileInfo.TYPE.SIGHT == t.Type);
            
            Random r = new Random();
            while (FeeBoostedTiles.Count < 4)
            {
                int next = r.Next(0, citiesAndSights.Count());

                StageTile citiesAndSight = citiesAndSights.Skip(next).Take(1).Last();
                
                if ( citiesAndSight.FeeBoosted ) continue;
                
                citiesAndSight.FeeBoosted = true;
                FeeBoostedTiles.Add(citiesAndSight.Position);
            }
        }

        public List<short> FeeBoostedTiles {get; set;}
        
    }
}