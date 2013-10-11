using System;
using System.Collections.Generic;
using DragonMarble.Message;
using log4net;

namespace DragonMarble {
    public class GameMaster
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameMaster));
        private List<StageTile> _tiles;
        private List<GamePlayer> _players;
        private StageManager _stageManager;

        public GameMaster(List<StageTile> tiles)
        {
            Id = Guid.NewGuid();
            _tiles = tiles;
            _players = new List<GamePlayer>();
        }

        public Guid Id { get; set; }

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
            
              //init game
            GameMessage boardMessage = new GameMessage
            {
                Header = new GameMessageHeader
                {
                    From = Guid.NewGuid(),
                    To = Guid.NewGuid()
                },
                Body = new GameMessageBody
                {
                    MessageType = GameMessageType.InitilizeBoard,
                    Content = new InitializeContent()
                    {
                        FeeBoostedTiles = new[] { 2, 3, 4, 4 }
                    }
                }
            };

            _players.ForEach(p=>p.SendingMessage=boardMessage);
        }

        public void Notify(Guid senderGuid, GameMessageType messageType, object messageContent)
        {
            
        }
    }
}