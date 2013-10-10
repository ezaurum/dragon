using System;
using System.Collections.Generic;
using Dragon.Server;

namespace DragonMarble {
    public class GameMaster : IActionRunner, IObservable<GameObject>
    {   
        private IList<StageTile> _tiles;
        private IList<GamePlayer> _players;        

        public GameMaster(IList<StageTile> tiles)
        {
            Tiles = tiles;
            _players = new List<GamePlayer>();
        }

        public IList<StageTile> Tiles
        {   
            set { _tiles = value; }
        }

        public AsyncUserToken NewAsyncUserToken()
        {
            GamePlayer gamePlayer = new GamePlayer 
            {GameMaster = this};

            Subscribe(gamePlayer);

            return gamePlayer;
        }

        public IDisposable Subscribe(IObserver<GameObject> observer)
        {
            lock (_players)
            {
                _players.Add((GamePlayer) observer);
            }
            return null;
        }
    }

    public class GameObject
    {

    }
}