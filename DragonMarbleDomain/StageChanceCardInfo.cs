using System;
using System.Collections;
using System.Collections.Generic;

namespace DragonMarble {
	public class StageChanceCardInfo {
		
		public enum TYPE {
			GOTO, BUFF, ORDER, COUPON
		}
		public enum ORDER_TYPE {
			GO_OLYMPIC_CITY, GO_ISLAND, OPEN_OLYMPIC,
			DONATE_CITY, DONATE_MONEY, CHANGE_CITY, SELL
		}
		
		public int classId;
		public string fileName;
		public TYPE type;
        public int tileIndex;

        public StageBuffInfo.TYPE buffType;
        public StageBuffInfo.TARGET buffTarget;
        public int buffPower;
        public int buffTurn;
        public string effectName;

        public ORDER_TYPE orderType;

        public StageUnitInfo.CHANCE_COUPON couponType;
		
		public StageChanceCardInfo(Hashtable data){
			classId = (int) data["ClassID"];
			fileName = (string) data["FileName"];
			Hashtable effectData = ((List<Hashtable>) data["Effect"])[0];
			type = (TYPE) Enum.Parse( typeof( TYPE ), (string) effectData["Type"] );

		    switch (type)
            {
                case TYPE.BUFF:
                    buffType = (StageBuffInfo.TYPE) Enum.Parse( typeof(StageBuffInfo.TYPE), (string) effectData["BuffType"] );
                    buffTarget = (StageBuffInfo.TARGET) Enum.Parse( typeof(StageBuffInfo.TARGET), (string) effectData["Target"] );
                    buffPower = (int) effectData["BuffPower"];
                    buffTurn = (int) effectData["BuffTurn"];
                    effectName = (string) effectData["EffectName"];
                    break;
                case TYPE.GOTO:
                    tileIndex = (int)effectData["TileIndex"];
                    break;
                case TYPE.COUPON:
                    couponType = (StageUnitInfo.CHANCE_COUPON)Enum.Parse(typeof(StageUnitInfo.CHANCE_COUPON), (string)effectData["CouponType"]);
                    break;
                case TYPE.ORDER:
                    orderType = (ORDER_TYPE)Enum.Parse(typeof(ORDER_TYPE), (string)effectData["OrderType"]);
		            break;
		    }

		}

	    public StageChanceCardInfo()
	    {
	        
	    }
		
		public static StageChanceCardInfo MakeCard(Hashtable data){
			return new StageChanceCardInfo(data);
		}

        public static StageChanceCardInfo MakeCard(int cardNo)
        {
            return null;
        }
		
	}
}