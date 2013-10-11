using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DragonMarble
{
    public class StageTile
    {   
        private readonly string _typeValue;
        private readonly int[] _buyPrices;
        private readonly int[] _sellPrices;
        private readonly int[] _fees;

        private readonly StageTileInfo _info;

        public StageTile(int index, string name, string type, string typeValue,
            int[] buyPrices, int[] sellPrices, int[] fees)
        {   
            _typeValue = typeValue;
            _buyPrices = buyPrices;
            _sellPrices = sellPrices;
            _fees = fees;
            _info = new StageTileInfo(index, name, 
                (StageTileInfo.TYPE) Enum.Parse(typeof(StageTileInfo.TYPE), type));
            _info.buildings = new List<StageTileInfo.Building>();
            
            for (int i =0; i < buyPrices.Length ; i++)
            {
                _info.buildings.Add(new StageTileInfo.Building()
                {buyPrice = buyPrices[i],
                    fee = fees[i],
                    sellPrice = sellPrices[i]
                    
                });
            }
        }

        public GameActionResult Result { get; set; }

        public static List<StageTile> Parse(XDocument doc)
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