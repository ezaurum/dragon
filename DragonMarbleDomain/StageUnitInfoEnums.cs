namespace DragonMarble
{
    public partial class  StageUnitInfo
    {
        public enum CHANCE_COUPON
        {
            NULL,
            DISCOUNT_50,
            ESCAPE_ISLAND,
            SHIELD,
            ANGEL
        }

        public enum ControlModeType
        {
            Player,
            AI_0,
            AI_1,
            AI_2,
        }

        public enum TEAM_GROUP
        {
            A = 0,
            B,
            C,
            D
        }

        public enum UNIT_COLOR
        {
            RED = 0,
            BLUE,
            YELLOW,
            GREEN,
            PINK,
            SKY
        }
		
		public enum SPECIAL_STATE
		{
			NULL,
			PRISON,
			TRAVEL
		}
		
    }
}
