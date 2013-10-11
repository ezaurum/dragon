using System;
using System.Collections.Generic;

namespace DragonMarble {
    public class GameMaster
    {
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

        public void StartGame()
        {
            _stageManager = new StageManager(_tiles, _players);
        }

        public void Join(GamePlayer player)
        {
            _players.Add(player);
        }
    }
}