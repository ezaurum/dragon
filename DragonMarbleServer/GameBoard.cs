using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace DragonMarble
{
    public class GameBoard
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameBoard));

        private readonly List<StageTile> _tiles;

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

            Logger.Debug("init board.");
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("{0},{1},{2},{3}", FeeBoostedTiles[0], FeeBoostedTiles[1], FeeBoostedTiles[2], FeeBoostedTiles[3]);
            }
            
        }

        public List<short> FeeBoostedTiles {get; set;}
        
    }
}