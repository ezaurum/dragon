using System;
using System.Linq;
using System.Threading.Tasks;
using DragonMarble.Message;

namespace DragonMarble
{
    public partial class GameMaster
    {
        public bool IsPublicGame { get; set; }

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

            /*player.SendingMessage  = new GoToWaitingRoomGameMessage
            {
                GameRoomId = Id
            };*/

            IDragonMarbleGameMessage message = new NewPlayerInWaitingRoomGameMessage
            {
                PlayerId = player.Id,
                Order = (short) player.Order

            };
            Notify(message);

            if (Units.Count < 1)
            {
                player.IsRoomOwner = true;
            }

            Units.Add(player.Id, player);
            
            Notify(new AssignRoomOwnerGameMessage
            {
                RoomOwner = Units.Values.ToArray()[0].Id
            });

            Notify(new WaitingRoomInfoGameMessage
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
                Notify(new AssignRoomOwnerGameMessage
                {
                    RoomOwner = Units.Values.ToArray()[0].Id
                });
            }

            Notify(new WaitingRoomInfoGameMessage
            {
                
                Units = Units.Values.ToList(),
                BoardType = 0,
                CurrentNumberOfPlayers = (short)Units.Count,
                NumberOfPlayers = PlayerNumberForPlay
            });
        }
    }
}
