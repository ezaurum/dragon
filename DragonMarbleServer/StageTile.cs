using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DragonMarble
{
    public class StageTile
    {   

        private readonly StageTileInfo _info;
        public int GroupId { get; set; }

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