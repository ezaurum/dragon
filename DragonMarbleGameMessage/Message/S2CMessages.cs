using System;
using System.Collections.Generic;

namespace DragonMarble.Message
{
    public class InitializeContentS2C : IGameMessageContent
    {
        public List<int> FeeBoostedTiles = new List<int>()
        {
            1, 9, 30, 28
        };
        
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[FeeBoostedTiles.Count * sizeof(int)];
            int index = 0;
            foreach (int feeBoostedTile in FeeBoostedTiles)
            {
                BitConverter.GetBytes(feeBoostedTile).CopyTo(bytes, index);
                index += sizeof(int);
            }
            return bytes;
        }
    }

    public class RollMoveDiceContentS2C : IGameMessageContent
    {
        public List<int> FeeBoostedTiles = new List<int>()
        {
            1, 9, 30, 28
        };

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[FeeBoostedTiles.Count * sizeof(int)];
            int index = 0;
            foreach (int feeBoostedTile in FeeBoostedTiles)
            {
                BitConverter.GetBytes(feeBoostedTile).CopyTo(bytes, index);
                index += sizeof(int);
            }
            return bytes;
        }
    }
}