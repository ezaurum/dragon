using System;
using System.Collections.Generic;

namespace DragonMarble
{
    public class GameBoard
    {
        private readonly List<StageTile> _tiles;

        public GameBoard(List<StageTile> tiles)
        {
            _tiles = tiles;
        }

        public List<short> FeeBoostedTiles
        {
            get
            {
                return new List<short>(new Int16[] {3, 31, 29, 4});
            }
        }
    }
}