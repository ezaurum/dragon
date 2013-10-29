using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DragonMarble
{
    public partial class GameMaster
    {
        public static List<StageTileInfo> ParseTiles(XDocument doc)
        {
            // Query the data and write out a subset of contacts
            IEnumerable<StageTileInfo> query = doc.Elements("Category").Elements("Stage").Select(c =>
            {
                IEnumerable<XElement> xElements = c.Elements("Price");

                int[] buyPrices = new int[4];
                int[] sellPrices = new int[4];
                int[] fees = new int[4];

                int i = 0;
                foreach (XElement xElement in xElements)
                {
                    buyPrices[i] = Int32.Parse(xElement.Attribute("BuyPrice").Value.ToString());
                    fees[i] = Int32.Parse(xElement.Attribute("Fee").Value.ToString());
                    sellPrices[i] = Int32.Parse(xElement.Attribute("SellPrice").Value.ToString());
                }

                return new StageTileInfo(
                    Int32.Parse(c.Attribute("Index").Value.ToString()),
                    c.Attribute("Name").Value.ToString(),
                    c.Attribute("Type").Value.ToString(),
                    c.Attribute("TypeValue").Value.ToString(), buyPrices, sellPrices, fees);
            });
            return query.ToList();
        }
    }
}