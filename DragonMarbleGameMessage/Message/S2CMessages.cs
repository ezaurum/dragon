using System;
using GameUtils;

namespace DragonMarble.Message
{
    public class InitializeContentS2C : IGameMessageContent
    {
        private int[] _feeBoostedTiles =
        {
            1, 9, 30, 28
        };

        public int[] FeeBoostedTiles {get { return _feeBoostedTiles; }set { _feeBoostedTiles = value; }}
        
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[_feeBoostedTiles.Length * sizeof(int)];
            int index = 0;
            foreach (int feeBoostedTile in FeeBoostedTiles)
            {
                BitConverter.GetBytes(feeBoostedTile).CopyTo(bytes, index);
                index += sizeof(int);
            }
            return bytes;
        }

        public void FromByteArray(byte[] bytes, int index = 0)
        {
            _feeBoostedTiles = new int[4];
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[0]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[1]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[2]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[3]);
        }
    }

    public class RollMoveDiceContentS2C : IGameMessageContent
    {
        private bool _doublePip;
        public bool DoublePip {get { return _doublePip; } set { _doublePip = value; }}
        public char[] Dices { get; set; }
        private char[] _dices;
        private const int ByteLength = sizeof(char) * 2 + sizeof(bool);
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[ByteLength];
            BitConverter.GetBytes(Dices[0]).CopyTo(bytes,0);
            BitConverter.GetBytes(Dices[1]).CopyTo(bytes,1);
            BitConverter.GetBytes(DoublePip).CopyTo(bytes,2);
            return bytes;
        }

        public void FromByteArray(byte[] bytes, int index = 0)
        {
            _dices = new char[2];
            BitConvertUtils.ReadBytes(bytes, ref index, ref _dices[0]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _dices[1]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _doublePip);
        }
    }
}