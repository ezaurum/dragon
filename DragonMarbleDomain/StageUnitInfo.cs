using System;
using System.Collections.Generic;
using Dragon.Interfaces;
using DragonMarble.Message;

namespace DragonMarble
{
    public class StageUnitInfo
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

        public CHANCE_COUPON chanceCoupon;

        public int gold;
        public Dictionary<int, StageTileInfo> lands;
        public int ranking;
        public int round;
        public TEAM_GROUP teamGroup;
        public int tileIndex;
        public StageBuffInfo unitBuff;
        public UNIT_COLOR unitColor;
        public int usableLoanCount;

        public StageUnitInfo(UNIT_COLOR unitColor, int initialCapital = 2000000)
            : this()
        {
            Id = Guid.NewGuid();
            this.unitColor = unitColor;
            gold = initialCapital;
        }

        public StageUnitInfo()
        {
            CharacterId = 1;
            round = 1;
            tileIndex = 0;
            lands = new Dictionary<int, StageTileInfo>();
            unitBuff = null;
            chanceCoupon = CHANCE_COUPON.NULL;
            usableLoanCount = 1;
            DiceId = 1;
            Dice = new StageDiceInfo();
        }

        public virtual IMessageProcessor<IDragonMarbleGameMessage> MessageProcessor { get; set; }
        public virtual IStageManager StageManager { get; set; }
        public StageDiceInfo Dice { get; set; }

        public virtual IDragonMarbleGameMessage ReceivedMessage
        {
            get { return MessageProcessor.ReceivedMessage; }
        }

        public virtual IDragonMarbleGameMessage SendingMessage
        {
            set { MessageProcessor.SendingMessage = value; }
        }

        public int Order { get; set; }
        public int ActionRemined { get; set; }


        public bool OwnTurn { get; set; }
        public int DiceId { get; set; }
        public ControlModeType ControlMode { get; set; }
        public int CharacterId { get; set; }
        public Guid Id { get; set; }

        public int property
        {
            get
            {
                int p = gold;
                foreach (StageTileInfo t in lands.Values)
                {
                    p += t.sellPrice;
                }
                return p;
            }
        }

        public bool isAbleToLoan
        {
            get
            {
                if (usableLoanCount > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public int Assets
        {
            get { return property; }
        }

        public int LastSelected { get; set; }

        public int Position
        {
            get { return tileIndex; }
            set { tileIndex = value; }
        }

        public int Gold
        {
            get { return gold; }
            set { gold = value; }
        }

        public UNIT_COLOR UnitColor
        {
            get { return unitColor; }
            set { unitColor = value; }
        }

        public void ResetMessages()
        {
            MessageProcessor.ResetMessages();
        }

        public bool AddGold(int a)
        {
            if (gold + a < 0) return false;
            gold += a;
            return true;
        }

        public int DonateMoney(int g)
        {
            if (g > gold) g = gold;
            AddGold(-g);
            return g;
        }

        public void UpdateTurn()
        {
            if (unitBuff != null)
            {
                unitBuff.UpdateTurn();
                if (!unitBuff.isAlive)
                {
                    unitBuff = null;
                }
            }
        }

        public void Go(int step)
        {
            tileIndex += step;
            if (tileIndex >= 32) tileIndex -= 32;
        }

        public bool Loan(int loanGold)
        {
            if (usableLoanCount > 0)
            {
                usableLoanCount--;
                AddGold(loanGold);
                return true;
            }
            return false;
        }

        public bool Pay(StageTileInfo tile)
        {
            int fee = tile.fee;
            if (unitBuff != null)
            {
                if (unitBuff.type == StageBuffInfo.TYPE.OVERCHARGE)
                {
                    fee += (fee*unitBuff.power/100);
                }
                else if (unitBuff.type == StageBuffInfo.TYPE.DISCOUNT)
                {
                    fee -= (fee*unitBuff.power/100);
                }
            }

            if (AddGold(-fee))
            {
                tile.owner.AddGold(fee);
                return true;
            }
            return false;
        }

        public int GetTax(int taxPercent)
        {
            int p = 0;
            foreach (StageTileInfo t in lands.Values)
            {
                p += t.builtPrice;
            }
            return p*taxPercent/100;
        }

        public void AddBuff(StageBuffInfo.TYPE buffType, int buffTurn, int power)
        {
            if (buffTurn > 0)
            {
                unitBuff = new StageBuffInfo(buffType, buffTurn, power);
            }
        }


        public void ActivateTurn()
        {
            OwnTurn = true;
        }

        public void DeactivateTurn()
        {
            OwnTurn = false;
            Dice.Clear();
        }


        public IEnumerable<GameAction> Actions()
        {
            for (ActionRemined = 1; ActionRemined > 0; ActionRemined--)
            {
                var action = new GameAction {PlayerNumber = Order, Actor = this};

                IDragonMarbleGameMessage receivedMessage = ReceivedMessage;

                DoAction(receivedMessage);

                switch (receivedMessage.MessageType)
                {
                    case GameMessageType.RollMoveDice:
                        var rollMoveDiceGameMessage = (RollMoveDiceGameMessage) receivedMessage;
                        Go((int) rollMoveDiceGameMessage.Pressed);
                        Console.WriteLine("{0}", Dice);
                        var
                            rmdrgm = new RollMoveDiceResultGameMessage
                            {
                                From = StageManager.Id,
                                To = Id,
                                Dices = new List<Char> {(char) Dice.result[0], (char) Dice.result[1]}
                            };
                        if (Dice.isDouble && Dice.rollCount < 3) ActionRemined += 1;
                        SendingMessage = rmdrgm;
                        break;
                }

                yield return action;
            }
        }

        public void DoAction(IDragonMarbleGameMessage message)
        {
            switch (message.MessageType)
            {
                case GameMessageType.RollMoveDice:
                    var rollMoveDiceGameMessage = (RollMoveDiceGameMessage) message;
                    //RollMoveDice(rollMoveDiceGameMessage.Pressed);
                    Console.WriteLine("{0}", Dice);
                    var
                        rmdrgm = new RollMoveDiceResultGameMessage
                        {
                            From = StageManager.Id,
                            To = Id,
                            Dices = new List<char> {(char) Dice.result[0], (char) Dice.result[1]}
                        };
                    if (Dice.isDouble && Dice.rollCount < 3) ActionRemined += 1;
                    //SendingMessage = rmdrgm;
                    break;

                case GameMessageType.RollMoveDiceResult:
                {
                    var rollMoveDiceResultGameMessage = (RollMoveDiceResultGameMessage) message;
                    int diceSum = 0;
                    foreach (char i in rollMoveDiceResultGameMessage.Dices) diceSum += i;
                    Go(diceSum);


                    break;
                }
                case GameMessageType.OrderCardSelect:
                {
                    var m = (OrderCardSelectGameMessage) message;
                    Guid guid = m.To;
                    m.To = m.From;
                    m.Actor = Id;
                    m.OrderCardSelectState = new List<bool> {true, true};
                    SendingMessage = m;
                    break;
                }
            }
        }
    }
}