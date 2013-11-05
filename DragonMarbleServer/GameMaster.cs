using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster
    {
        public GameMaster(Byte boardType, GamePlayType gamePlayType)
        {
            BoardType = boardType;
            GamePlayType = gamePlayType;
            if (GamePlayType.TeamPlay == gamePlayType)
            {
                PlayerNumberForPlay = 4;
            }
            else
            {
                PlayerNumberForPlay = (short) gamePlayType;
            }


            _state = GameState.JustMade;
            Id = Guid.NewGuid();
            Units = new Dictionary<Guid, StageUnitInfo>();
            Cards = new Dictionary<int, StageChanceCardInfo>();
            ChanceCardList.ForEach(c => Cards.Add(c.classId, c));
        }
        
        public void StartGame()
        {
            InitGame();
            EndOrder();
            Logger.Debug("Start Game End");
        }

        private void InitGame()
        {
            _availablePlayers = Units.Values.ToList();
            Board = OriginalBoard.Clone();

            Board.Init();

            //send initialize message
            Notify(new InitializeGameGameMessage
            {
                FeeBoostedTiles = Board.FeeBoostedTiles,
                NumberOfPlayers = (short) Units.Count,
                Units = Units.Values.ToList()
            });

            _state = GameState.Init;

            GameContinue = true;

            //order
            //At first, select order
            _receiveMessageWaitHandler.Reset();

            OrderCardSelectGameMessage orderCardSelectGameMessage = new OrderCardSelectGameMessage
            {
                Actor = Id,
                NumberOfPlayers = (short)Units.Count,
                OrderCardSelectState = new List<bool>(),
                SelectedCardNumber = -1
            };

            for (int i = 0; i < Units.Count; i++)
            {
                orderCardSelectGameMessage.OrderCardSelectState.Add(false);
            }
            _state = GameState.OrderPlayers;
            Notify(orderCardSelectGameMessage);
        }

        private void EndOrder()
        {
            _receiveMessageWaitHandler.WaitOne();
            ///pick random card
            short firstCardNumber = (short) RandomUtil.Next(0,Units.Count);
            Guid firstPlayerId = _orderCard[firstCardNumber];
            
            Notify(new OrderCardResultGameMessage
            {
                FirstPlayerId = firstPlayerId,
                FirstCardNumber = firstCardNumber
            });

            OrderedByTurnPlayers = Units.Values.OrderBy(player => player.Order).ToList();

            Logger.Debug("End order");

            //playe game is need another thread.
            Task.Factory.StartNew(ProcessAction);
        }
    }
}