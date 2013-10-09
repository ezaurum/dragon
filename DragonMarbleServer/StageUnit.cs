using System;
using System.Collections.Generic;

namespace DragonMarble
{
    public class StageUnit
    {
        public StageManager StageManager { get; set; }
        public UnitState State;
        private GameActionResult _result;
        private int _position;
        public StageDiceInfo Dice { get; set; }

        public int Position
        {
            get
            {
                return _position;
            }
        }


        public StageUnit(StageUnitInfo.TEAM_COLOR teamColor, int initialCapital, string guid)
        {
            Id = guid;
            Info = new StageUnitInfo(teamColor, initialCapital);
            Dice = new StageDiceInfo();
        }

        public string Id { get; set; }

        public StageUnitInfo Info { get; set; }

        private int RollMoveDice(int press)
        {
            Dice.Roll();
            _position += Dice.resultSum;
            return Position;
        }

        public int Gold
        {
            get
            {
                return Info.gold;
            }
            set
            {
                Info.gold = value;
            }
        }

        public StageUnitInfo.TEAM_COLOR TeamColor
        {
            get
            {
                return Info.teamColor;
            }
        }

        public int Order
        {
            get
            {
                return Info.Order;
            }
            set
            {
                Info.Order = value;
            }
        }

        public int Capital
        {
            get
            {
                return Info.Capital;
            }
            set
            {
                Info.Capital = value;
            }
        }

        public int CapitalOrder
        {
            get
            {
                return Info.CapitalOrder;
            }
            set
            {
                Info.CapitalOrder = value;
            }
        }

        public int LastSelected { get; set; }

        public GameActionResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                SetResult(value);
            }
        }

        public bool Earn(int a)
        {
            return Info.AddGold(a);
        }

        public bool Pay(int money)
        {
            return Info.AddGold(-money);
        }

        public void ActivateTurn()
        {
            Console.WriteLine("My turn!");
            Info.OwnTurn = true;
        }

        public IEnumerable<GameAction> GetAction()
        {
            Console.WriteLine("I Do something...");
            yield return new GameAction();
        }
        public void SetResult(GameActionResult result)
        {
            Console.WriteLine("Set Action result.");
        }

        public void DeactivateTurn()
        {
            Info.OwnTurn = false;
        }

        public void StartTurn(int turn, GameActionType actionType,
            bool active = true, GameActionResult result = null)
        {
            Info.OwnTurn = active;

            switch (actionType)
            {
                case GameActionType.OrderCardSelect:

                    break;
                case GameActionType.RollDice:
                    break;
                case GameActionType.SelectDestination:
                    break;
            }

            if (!active && null != result)
            {

            }

        }

        public IEnumerable<GameAction> Actions()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("this is actions in player {1}. {0}", i, Order);
                GameAction action = new GameAction { PlayerNumber = Order };

                //need something to stop running.

                yield return action;
            }
        }
    }

    public enum UnitState
    {

    }
}