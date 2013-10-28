using System;
using System.Collections.Generic;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public static class RandomFactory
    {
        public static Random NewRandom()
        {
            return new Random();
        }
    }
	public static class RandomUtil
	{
		public static float Next(float min, float max){
            return RandomFactory.NewRandom().Next((int)(min * 1000), (int)max * 1000) / 1000f;
		}
	}
	

    public class StageBuffInfo
    {
        public enum TARGET
        {
            OWNER,
            CITY_ENEMY,
            CITYGROUP_ENEMY,
            BUILDING_ENEMY,
            BUILDINGGROUP_ENEMY
        }

        public enum TYPE
        {
            OVERCHARGE,
            DISCOUNT,
            DESTROY
        }

        public int power;
        public int turn;
        public TYPE type;

        public StageBuffInfo(TYPE type, int power, int turn)
        {
            this.type = type;
            this.turn = turn;
            this.power = power;
        }

        public bool isAlive
        {
            get
            {
                if (turn >= 0)
                {
                    return true;
                }
                return false;
            }
        }

        public void UpdateTurn()
        {
            turn--;
        }
    }

    public class StageDiceInfo
    {

        public static int diceCheat;

        public enum ROLL_TYPE
        {
            NORMAL,
            ODD,
            EVEN
        }

        private readonly Random rand;
        public bool isDouble;

        public int[] result;
        public int rollCount;
        public ROLL_TYPE rollType;

        public StageDiceInfo()
        {
            rand = RandomFactory.NewRandom();
            result = new[] { 0, 0 };
            rollCount = 0;
            isDouble = false;
            rollType = ROLL_TYPE.NORMAL;
        }

        public int resultSum
        {
            get
            {
                int s = 0;
                foreach (int r in result)
                {
                    s += r;
                }
                return s;
            }
        }

        public void Roll()
        {
            result[0] = rand.Next(1, 7);
            result[1] = rand.Next(1, 7);

            int sum = result[0] + result[1];
            if ((rollType == ROLL_TYPE.ODD && sum % 2 == 0) || (rollType == ROLL_TYPE.EVEN && sum % 2 == 1))
            {
                if (result[0] == 1)
                {
                    result[0]++;
                }
                else if (result[0] == 6)
                {
                    result[0]--;
                }
                else
                {
                    int[] r = { 1, -1 };
                    result[0] += r[rand.Next(2)];
                }
            }


            if (diceCheat >= 2)
            {
                result[0] = (char)1;
                result[1] = (char)1;
                diceCheat -= 2;
                while (diceCheat > 0)
                {
                    diceCheat--;
                    int r = new Random().Next(0, 2);
                    if (result[r] == (char)6)
                    {
                        r++;
                        if (r == 2) r = 0;
                    }
                    result[r]++;
                }
            }

            
            if (result[0] == result[1])
            {
                isDouble = true;
            }
            else
            {
                isDouble = false;
            }
            rollType = ROLL_TYPE.NORMAL;
            rollCount++;
        }

        public void Clear()
        {
            rollCount = 0;
            isDouble = false;
            result[0] = 0;
            result[1] = 0;
        }

        public IGameMessage RollAndGetResultGameAction(StageUnitInfo actor, float pressed, bool odd, bool even)
        {
            Roll();
            return DiceGameAction(actor);
        }

        private IGameMessage DiceGameAction(StageUnitInfo stageUnitInfo)
        {
            return new RollMoveDiceResultGameMessage
            {
                Actor = stageUnitInfo.Id,
                Dices = new List<char> {(char) result[0], (char) result[1]}
            };
        }
    }


    public class StageGambleInfo
    {
        public const int MAX_WIN_COUNT = 3;
        public const int BASIC_SHOW_COUNT = 4;
        public const int COMPARER_NUMBER = 7;
        public enum TYPE
        {
            spade, dia, heart, clover
        }
        public enum CHOICE
        {
            NULL, HIGH, LOW
        }
        public enum RESULT
        {
            NULL, WIN, LOSE
        }

        public class CardData
        {
            public TYPE type;
            public int num;
            public CardData(TYPE type, int num)
            {
                this.type = type;
                this.num = num;
            }
        }
        Random rand;
        public int winCount;
        public int rewardScale;
        public int battingPrice;
        public int rewardPrice;
        public RESULT result;
        public CHOICE selectChoice;
        public List<CardData> cards;
        public List<CardData> useCards;

        public StageGambleInfo()
        {
            rand = RandomFactory.NewRandom();
            winCount = 0;
            battingPrice = 0;
            rewardPrice = 0;
            result = RESULT.NULL;
            selectChoice = CHOICE.NULL;
        }

        public void SetBattingPrice(int price)
        {
            if (winCount == 0)
            {
                battingPrice = price;
                rewardScale = 2;
            }
        }


        public void InitCards()
        {
            cards = new List<CardData>();
            foreach (TYPE t in Enum.GetValues(typeof(TYPE)))
            {
                for (int i = 1; i <= 13; i++)
                {
                    cards.Add(new CardData(t, i));
                }
            }
            useCards = new List<CardData>();
        }

        public void UseBasicCards()
        {
            for (int i = 0; i < BASIC_SHOW_COUNT; i++)
            {
                PickOneCard();
            }
        }

        CardData PickOneCard()
        {
            int r = rand.Next(0, cards.Count);
            useCards.Add(cards[r]);
            cards.RemoveAt(r);
            return useCards[useCards.Count - 1];
        }

        public CardData SelectChoice(CHOICE c)
        {
            selectChoice = c;
            CardData card = PickOneCard();
            if ((c == CHOICE.HIGH && card.num >= COMPARER_NUMBER) || (c == CHOICE.LOW && card.num <= COMPARER_NUMBER))
            {
                result = RESULT.WIN;
                rewardPrice = battingPrice * rewardScale;
                winCount++;
                if (winCount < MAX_WIN_COUNT)
                {
                    rewardScale = rewardScale * 2;
                }

            }
            else
            {
                result = RESULT.LOSE;
                rewardPrice = 0;
            }
            return card;
        }

    }
}