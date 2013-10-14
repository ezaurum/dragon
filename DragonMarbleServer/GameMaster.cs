using System;
using System.Collections.Generic;
using System.Linq;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public class GameMaster
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (GameMaster));
        private readonly List<GamePlayer> _players;
        private readonly List<StageTile> _tiles;
        private StageManager _stageManager;

        public GameMaster(List<StageTile> tiles)
        {
            Id = Guid.NewGuid();
            _tiles = tiles;
            _players = new List<GamePlayer>();
        }

        public Guid Id { get; set; }

        public bool IsGameStartable
        {
            get { return (_players.Count > 1); }
        }

        public void Join(GamePlayer player)
        {
            _players.Add(player);
            player.GameMaster = this;
        }

        public void StartGame()
        {
            Logger.Debug("Start game");

            _stageManager = new StageManager(_tiles, _players);

            _stageManager.InitGame();
            _stageManager.StartGame();
        }

        public void Notify(Guid senderGuid,
            GameMessageType messageType,
            IGameMessageContent messageContent)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Notify from {0}, messageType:{1}"
                    , senderGuid, messageType);
            }

            foreach (GamePlayer p in _players.Where(p => p.Id != senderGuid))
            {
                p.SendingMessage
                    = new GameMessage(Id, p.Id, senderGuid, messageType, messageContent);
            }
        }
    }
}