using System;
using System.Linq;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster : IStageManager
    {
        public void ActionResultCopySended()
        {
            if (_availablePlayers.All(
                availablePlayer => availablePlayer.IsActionResultCopySended
                ))
            {
                _receiveMessageWaitHandler.Set();
            }
        }

        public void ReadyNotify(ReadyStateGameMessage message)
        {   
            Notify(message);
            if (IsStartable)
            {
                Notify(new EveryoneIsReadyGameMessage
                {
                    ResponseLimit = 100000
                });
            }
        }

        public void OrderSelectSended(OrderCardSelectGameMessage message)
        {
            short selectedCardNumber = message.SelectedCardNumber;
            lock (_orderCard)
            {
                if (_orderCard.ContainsKey(selectedCardNumber))
                {
                    message.SelectedCardNumber = -1;
                }
                else
                {
                    _orderCard.Add(selectedCardNumber, message.Actor);
                    message.OrderCardSelectState[selectedCardNumber] = true;
                }
            }

            Notify(message);

            if (_orderCard.Count == _availablePlayers.Count - 1)
            {
                short notSelectedCard = 0;
                Guid notSelectedUnit = Guid.Empty;

                for (short i = 0; i < _availablePlayers.Count; i++)
                {
                    if (_orderCard.ContainsKey(i)) continue;

                    notSelectedCard = i;
                    break;
                }

                foreach (StageUnitInfo stageUnitInfo in _availablePlayers)
                {
                    if(_orderCard.ContainsValue(stageUnitInfo.Id)) continue;
                    notSelectedUnit = stageUnitInfo.Id;
                    break;
                }
                
                if ( notSelectedUnit == Guid.Empty )
                    throw new InvalidOperationException("empty guid!");

                _orderCard.Add(notSelectedCard, notSelectedUnit);
                message.Actor = notSelectedUnit;
                message.SelectedCardNumber = notSelectedCard;
                message.OrderCardSelectState[notSelectedCard] = true;
                Notify(message);
            }

            if (_orderCard.Count == _availablePlayers.Count)
            {
                _receiveMessageWaitHandler.Set();
            }
        }

        //TODO remove
        public static GameMaster Temp { get; set; }
    }
}