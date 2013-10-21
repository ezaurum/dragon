using System;
using System.Collections.Generic;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public class GamePlayer : StageUnitInfo
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (GamePlayer));

        public GamePlayer(UNIT_COLOR teamColor, int initialCapital)
            : base(teamColor, initialCapital)
        {

        }

        public GamePlayer()
        {
            
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
            Logger.Debug("Activated Turn");
            OwnTurn = true;
        }

        public void DeactivateTurn()
        {
            OwnTurn = false;
            Dice.Clear();
        }

        public void StartTurn(int turn, GameMessageType actionType,
            bool active = true, GameActionResult result = null)
        {
            OwnTurn = active;

            switch (actionType)
            {
                case GameMessageType.OrderCardSelect:

                    break;
                case GameMessageType.RollMoveDice:
                    break;
            }

            if (!active && null != result)
            {
            }
        }

        public IEnumerable<GameAction> Actions()
        {
            for ( ActionRemined = 1; ActionRemined > 0; ActionRemined--)
            {
                Console.WriteLine("this is actions in id : {2}, player order {0} mode : {1}",  Order, ControlMode, Id);
                var action = new GameAction {PlayerNumber = Order, Actor = this};

                //need something to stop running.
                SendingMessage = new ActivateTurnGameMessage
                {
                    To = Id,
                    From = StageManager.Id,
                    TurnOwner = Id,
                    ResponseLimit = 50000
                };

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
                
                Logger.Debug( receivedMessage.MessageType);

                yield return action;
            }
        }
    }
}