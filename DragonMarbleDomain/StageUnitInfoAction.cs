using System;
using System.Collections.Generic;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class StageUnitInfo
    {
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
                var action = new GameAction { PlayerNumber = Order, Actor = this };

                IDragonMarbleGameMessage receivedMessage = ReceivedMessage;

                DoAction(receivedMessage);

                switch (receivedMessage.MessageType)
                {
                    case GameMessageType.RollMoveDice:
                        var rollMoveDiceGameMessage = (RollMoveDiceGameMessage)receivedMessage;
                        Go((int)rollMoveDiceGameMessage.Pressed);
                        Console.WriteLine("{0}", Dice);
                        var
                            rmdrgm = new RollMoveDiceResultGameMessage
                            {
                                From = StageManager.Id,
                                To = Id,
                                Dices = new List<Char> { (char)Dice.result[0], (char)Dice.result[1] }
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
                    var rollMoveDiceGameMessage = (RollMoveDiceGameMessage)message;
                    //RollMoveDice(rollMoveDiceGameMessage.Pressed);
                    Console.WriteLine("{0}", Dice);
                    var
                        rmdrgm = new RollMoveDiceResultGameMessage
                        {
                            From = StageManager.Id,
                            To = Id,
                            Dices = new List<char> { (char)Dice.result[0], (char)Dice.result[1] }
                        };
                    if (Dice.isDouble && Dice.rollCount < 3) ActionRemined += 1;
                    //SendingMessage = rmdrgm;
                    break;

                case GameMessageType.RollMoveDiceResult:
                    {
                        var rollMoveDiceResultGameMessage = (RollMoveDiceResultGameMessage)message;
                        int diceSum = 0;
                        foreach (char i in rollMoveDiceResultGameMessage.Dices) diceSum += i;
                        Go(diceSum);


                        break;
                    }
                case GameMessageType.OrderCardSelect:
                    {
                        var m = (OrderCardSelectGameMessage)message;
                        Guid guid = m.To;
                        m.To = m.From;
                        m.Actor = Id;
                        m.OrderCardSelectState = new List<bool> { true, true };
                        SendingMessage = m;
                        break;
                    }
            }
        }
    }
}
