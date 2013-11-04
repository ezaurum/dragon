using System;
using System.Linq;
using System.Threading.Tasks;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster
    {
        public void StartGame(Guid owner)
        {
            if (IsStartable
                && Units.Values.Any(p => p.Id == owner) 
                )
            {
                Task.Factory.StartNew(StartGame);
            }
        }

        private bool IsStartable
        {
            get
            {
                return Units.Count == PlayerNumberForPlay
                       && Units.Values.All(p => p.IsReady);
            }
        }

        /// <summary>
        /// Join player to game waiting room
        /// </summary>
        /// <param name="player"></param>
        public void Join(StageUnitInfo player)
        {
            player.StageManager = this;
            player.Stage = Board;

            //set initailize player message
            player.SendingMessage = new InitializePlayerGameMessage
            {
                PlayerId = player.Id,
                Server = Id
            };

            IDragonMarbleGameMessage message = new NewPlayerJoinGameMessage
            {
                PlayerId = player.Id
            };
            Notify(message);

            if (Units.Count < 1)
            {
                player.IsRoomOwner = true;
            }

            Units.Add(player.Id, player);
            
            Notify(new RoomOwnerGameMessage()
            {
                RoomOwner = Units.Values.ToArray()[0].Id
            });

            Notify(new InitializeWaitingRoomGameMessage
            {
                Units = Units.Values.ToList(),
                BoardType = 0,
                CurrentNumberOfPlayers = (short)Units.Count,
                NumberOfPlayers = PlayerNumberForPlay
            });
        }

        /// <summary>
        /// Exit player from game waiting room
        /// </summary>
        /// <param name="player"></param>
        public void Exit(StageUnitInfo player)
        {
            IDragonMarbleGameMessage message = new ExitWaitingRoomGameMessage
            {
                Actor = player.Id
            };
            Notify(message);

            Units.Remove(player.Id);

            if (player.IsRoomOwner)
            {
                Units.Values.ToArray()[0].IsRoomOwner = true;
                Notify(new RoomOwnerGameMessage()
                {
                    RoomOwner = Units.Values.ToArray()[0].Id
                });
            }

            Notify(new InitializeWaitingRoomGameMessage
            {
                Units = Units.Values.ToList(),
                BoardType = 0,
                CurrentNumberOfPlayers = (short)Units.Count,
                NumberOfPlayers = PlayerNumberForPlay
            });
        }
    }
}
