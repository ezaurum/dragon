using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

        public static List<StageChanceCardInfo> ParseCards(XDocument doc)
        {
            // Query the data and write out a subset of contacts
            IEnumerable<StageChanceCardInfo> query = doc.Elements("Category").Elements("Chance").Select(c =>
            {
                int classId = Int32.Parse(c.Attribute("ClassID").Value);
                
                XElement element = c.Element("Effect");
                StageChanceCardInfo.TYPE cardType = (StageChanceCardInfo.TYPE) Enum.Parse(typeof(StageChanceCardInfo.TYPE), element.Attribute("Type").Value);

                StageChanceCardInfo stageChanceCardInfo = new StageChanceCardInfo
                {
                    classId = classId,
                    type = cardType
                };
           
                switch (cardType)
                {
                    case StageChanceCardInfo.TYPE.BUFF:
                        stageChanceCardInfo.buffType = (StageBuffInfo.TYPE)Enum.Parse(typeof(StageBuffInfo.TYPE), element.Attribute("BuffType").Value);
                        stageChanceCardInfo.buffTarget = (StageBuffInfo.TARGET)Enum.Parse(typeof(StageBuffInfo.TARGET), element.Attribute("Target").Value);
                        stageChanceCardInfo.buffPower = Convert.ToInt32(element.Attribute("BuffPower").Value);
                        stageChanceCardInfo.buffTurn = Convert.ToInt32(element.Attribute("BuffTurn").Value);
                        break;
                    case StageChanceCardInfo.TYPE.GOTO:
                        stageChanceCardInfo.tileIndex  = Int32.Parse(element.Attribute("TileIndex").Value);
                        break;
                    case StageChanceCardInfo.TYPE.COUPON:
                        stageChanceCardInfo.couponType = (StageUnitInfo.CHANCE_COUPON)Enum.Parse(typeof(StageUnitInfo.CHANCE_COUPON), element.Attribute("CouponType").Value);
                        break;
                    case StageChanceCardInfo.TYPE.ORDER:
                        stageChanceCardInfo.orderType = (StageChanceCardInfo.ORDER_TYPE)Enum.Parse(typeof(StageChanceCardInfo.ORDER_TYPE), element.Attribute("OrderType").Value);
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("there is no sutiable matching for type : {0}", cardType));
                }
                
                return stageChanceCardInfo;
            });
            return query.ToList();
        }
    }

    public static class GameBoardUtil
    {
        public static GameBoard Clone(this GameBoard gameBoard)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, gameBoard);
                    ms.Position = 0;

                    return (GameBoard) formatter.Deserialize(ms);
                }
                catch (Exception e)
                {
                    throw e;
                }
                
            }
        }
    }
}