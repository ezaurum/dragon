using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using log4net;

namespace DragonMarble
{
    public class GameBoard
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameBoard));

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

            Logger.Debug("init board.");
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("{0},{1},{2},{3}", FeeBoostedTiles[0], FeeBoostedTiles[1], FeeBoostedTiles[2], FeeBoostedTiles[3]);
            }
            
        }

        public List<short> FeeBoostedTiles {get; set;}

        public static List<StageTile> ParseTiles(XDocument doc)
        {
            // Query the data and write out a subset of contacts
            IEnumerable<StageTile> query = doc.Elements("Category").Elements("Stage").Select(c =>
            {
                IEnumerable<XElement> xElements = c.Elements("Price");

                var buyPrices = new int[4];
                var sellPrices = new int[4];
                var fees = new int[4];

                int i = 0;
                foreach (XElement xElement in xElements)
                {
                    buyPrices[i] = int.Parse(xElement.Attribute("BuyPrice").Value.ToString());
                    fees[i] = int.Parse(xElement.Attribute("Fee").Value.ToString());
                    sellPrices[i] = int.Parse(xElement.Attribute("SellPrice").Value.ToString());
                }

                return new StageTile(
                    int.Parse(c.Attribute("Index").Value.ToString()),
                    c.Attribute("Name").Value.ToString(),
                    c.Attribute("Type").Value.ToString(),
                    c.Attribute("TypeValue").Value.ToString(), buyPrices, sellPrices, fees);
            });
            return query.ToList();
        }
        
    }
}