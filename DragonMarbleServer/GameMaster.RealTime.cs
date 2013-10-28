using System.Linq;

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

        public void OrderSelectSended()
        {
            if (_orderCard.Count == _availablePlayers.Count - 1)
            {

            }
            if (_orderCard.Count == _availablePlayers.Count)
            {
                _receiveMessageWaitHandler.Set();
            }
        }

    }
}
