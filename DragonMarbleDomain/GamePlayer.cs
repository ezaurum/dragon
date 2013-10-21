using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public class GamePlayer : StageUnitInfo
    {
        public GamePlayer(UNIT_COLOR teamColor, int initialCapital)
            : base(teamColor, initialCapital)
        {

        }

        public GamePlayer()
        {
            
        }

        public int Assets
        {
            get
            {
                return property;
            }
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
            get
            {
                return unitColor;
            }
            set
            {
                unitColor = value;
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
            for ( ActionRemined = 1; ActionRemined > 0; ActionRemined--)
            {
                Console.WriteLine("this is actions in id : {2}, player order {0} mode : {1}",  Order, ControlMode, Id);
                var action = new GameAction {PlayerNumber = Order, Actor = this};

                IDragonMarbleGameMessage receivedMessage = ReceivedMessage;

                DoAction(receivedMessage);

                switch (receivedMessage.MessageType)
                {
                    case GameMessageType.RollMoveDice:
                        RollMoveDiceGameMessage rollMoveDiceGameMessage = (RollMoveDiceGameMessage)receivedMessage;
                        Go((int)rollMoveDiceGameMessage.Pressed);
                        Console.WriteLine("{0}", Dice);
                        RollMoveDiceResultGameMessage
                        rmdrgm = new RollMoveDiceResultGameMessage()
                        {
                            From = StageManager.Id,
                            To = Id,
                            Dices = new List<Char> { (char) Dice.result[0], (char) Dice.result[1] }
                        };
                        if (Dice.isDouble && Dice.rollCount < 3) ActionRemined += 1;
                        SendingMessage = rmdrgm;
                        break;
                }

                yield return action;
            }
        }
    }
}