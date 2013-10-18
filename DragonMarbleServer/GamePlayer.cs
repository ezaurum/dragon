using System;
using System.Collections.Generic;
using Dragon.Interfaces;
using Dragon.Server;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    internal class AIGamePlayer : GamePlayer
    {
        public AIGamePlayer() : this(TEAM_COLOR.BLUE, 2000000)
        {   
        }

        public AIGamePlayer(TEAM_COLOR teamColor, int initCapital) : base(teamColor, initCapital)
        {
            ControlMode = ControlModeType.AI_0;
            Token = new AIQueuedMessageProcessor();
            Dice = new StageDiceInfo();
        }

        public override GameMaster GameMaster
        {
            set
            {
                base.GameMaster = value;
                ((AIQueuedMessageProcessor) Token).GameMasterId = value.Id;
            }
        }
    }

    internal class AIQueuedMessageProcessor : QueuedMessageProcessor
    {
        public Guid GameMasterId { get; set; }
        public override IGameMessage SendingMessage
        {
            set
            {
                base.SendingMessage = value;
                Console.WriteLine("AI received.");
                ReceivedMessage = GetMessage(value);
            }
        }

        public override IGameMessage ReceivedMessage
        {   
            set
            {
                if ( null != value )
                    base.ReceivedMessage = value;
            }
        }

        private IDragonMarbleGameMessage GetMessage(IGameMessage message)
        {
            GameMessageType messageType = ((IDragonMarbleGameMessage)message).MessageType;
            IDragonMarbleGameMessage result = null;
            switch (messageType)
            {
                case GameMessageType.ActivateTurn:
                    result = new RollMoveDiceGameMessage()
                    {
                        Pressed = 10,
                        To = Id,
                        From = GameMasterId
                    };
                    break;
            }
            return result;
        }
    }


    public class GamePlayer : StageUnitInfo
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (GamePlayer));
        private GameActionResult _result;

        public GamePlayer()
        {
            Dice = new StageDiceInfo();
        }

        public GamePlayer(TEAM_COLOR teamColor, int initialCapital) : base(teamColor, initialCapital)
        {
            Dice = new StageDiceInfo();
        }
        
        public QueuedMessageProcessor Token { get; set; }
        public virtual GameMaster GameMaster { get; set; }
        public StageManager StageManager { get; set; }
        public StageDiceInfo Dice { get; set; }
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

        public GameActionResult Result
        {
            get { return _result; }
            set { SetResult(value); }
        }

        public virtual IGameMessage ReceivedMessage
        {
            get { return Token.ReceivedMessage; }
        }

        public virtual IGameMessage SendingMessage
        {
            set { Token.SendingMessage = value; }
        }

        public TEAM_COLOR TeamColor
        {
            get
            {
                return teamColor;
            }
            set
            {
                teamColor = value;
            }
        }

        private int RollMoveDice(int press)
        {
            Dice.Roll();
            Go(Dice.resultSum);
            return Position;
        }

        public bool Earn(int a)
        {
            return AddGold(a);
        }

        public bool Pay(int money)
        {
            return AddGold(-money);
        }

        public void ActivateTurn()
        {
            Console.WriteLine("My turn!");
            OwnTurn = true;
        }

        public void SetResult(GameActionResult result)
        {
            Console.WriteLine("Set Action result.");
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
                    From = GameMaster.Id,
                    TurnOwner = Id,
                    ResponseLimit = 50000
                };

                IDragonMarbleGameMessage receivedMessage = (IDragonMarbleGameMessage) ReceivedMessage;

                switch (receivedMessage.MessageType)
                {
                    case GameMessageType.RollMoveDice:
                        RollMoveDiceGameMessage rollMoveDiceGameMessage = (RollMoveDiceGameMessage)receivedMessage;
                        RollMoveDice(rollMoveDiceGameMessage.Pressed);
                        Console.WriteLine("{0}", Dice);
                        RollMoveDiceResultGameMessage
                        rmdrgm = new RollMoveDiceResultGameMessage()
                        {
                            From = GameMaster.Id,
                            To = Id,
                            Dices = new List<Int32> { Dice.result[0], Dice.result[1] }
                        };
                        if (Dice.isDouble && Dice.rollCount < 3) ActionRemined += 1;
                        SendingMessage = rmdrgm;
                        break;
                }
                
                Console.WriteLine( receivedMessage.MessageType);

                yield return action;
            }
        }

        public int ActionRemined { get; set; }
    }
}