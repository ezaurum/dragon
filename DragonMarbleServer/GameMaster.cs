using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dragon.Message;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster
    {
        public void StartGame()
        {
            InitGame();
            SendOrderCardSelectMessage();
            EndOrder();
            Logger.Debug("Starg gmae and");
        }

        private void InitGame()
        {
            _availablePlayers = Units;

            Board.Init();

            //send initialize message
            Notify(new InitializeGameGameMessage
            {
                FeeBoostedTiles = Board.FeeBoostedTiles,
                NumberOfPlayers = (short) Units.Count,
                Units = Units
            });

            _state = GameState.Init;
        }

        private void SendOrderCardSelectMessage()
        {
            if (GameState.Init != _state)
                throw new InvalidOperationException("State is not initialized.");

            _state = GameState.OrderPlayers;
            GameContinue = true;

            //order
            //At first, select order

            Notify(new OrderCardSelectGameMessage
            {
                Actor = Id,
                NumberOfPlayers = (short) Units.Count,
                OrderCardSelectState = new List<bool> {false, false},
                SelectedCardNumber = -1
            });
        }

        private void EndOrder()
        {
            Notify(new OrderCardResultGameMessage
            {
                FirstPlayerId = Units[0].Id,
                FirstCardNumber = 1
            });

            Units[0].Order = 0;
            Units[1].Order = 1;
            OrderedByTurnPlayers = Units.OrderBy(player => player.Order).ToList();

            Logger.Debug("End order");

            //playe game is need another thread.
            Task.Factory.StartNew(PlayGame);
        }

        private void PlayGame()
        {
            Logger.Debug("Play Game");
            ProcessAction();
            EndGame();
        }

        public void EndGame()
        {
            _state = GameState.EndGame;
            Logger.Debug("end game.");
        }

        private void ProcessAction()
        {
            Logger.Debug("Process action");

            foreach (IDragonMarbleGameMessage action in PlayerActions())
            {
                Board.GrossAssets = 0;
                Units.ForEach(p => Board.GrossAssets += p.Assets);
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Gross Assets is : {0}", Board.GrossAssets);
                }

                CurrentAction = action;
                _state = GameState.ProcessPlayerAction;
                if (GameMessageType.ActionResultCopy == action.MessageType)
                {
                    _receiveMessageWaitHandler.Reset();
                    _availablePlayers.ForEach(p => p.IsActionResultCopySended = false);
                }

                if (GameMessageType.ActionResultCopy != action.MessageType)
                {
                    Notify(action);
                }

                //wait for action result copy when message is action result copy
                if (GameMessageType.ActionResultCopy == action.MessageType)
                {
                    _receiveMessageWaitHandler.Reset();
                    Logger.DebugFormat("wait for action result copy Turn:{0}", Turn + 1);
                    _receiveMessageWaitHandler.WaitOne();
                }
            }
        }

        private IEnumerable<IGameMessage> PlayerActions()
        {
            Logger.Debug("Player actions");
            foreach (IGameMessage action
                in PlayersOrderByTurn().SelectMany(player => player.Actions()))
            {
                _state = GameState.WaitPlayerAction;

                Logger.DebugFormat("_state:{0}", _state);

                //turn owner's action
                yield return action;
            }
        }

        private IEnumerable<StageUnitInfo> PlayersOrderByTurn()
        {
            for (Turn = 0; Turn < TurnLimit; Turn++)
            {
                Logger.DebugFormat("start Turn:{0}", Turn + 1);

                Notify(CurrentPlayer.ActivateTurn());

                yield return CurrentPlayer;

                _receiveMessageWaitHandler.Reset();
            }
        }
    }
}