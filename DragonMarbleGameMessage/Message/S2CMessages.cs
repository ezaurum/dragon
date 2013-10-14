using System;
using System.Collections;
using System.Collections.Generic;
using GameUtils;

namespace DragonMarble.Message
{
    public class PlayersInformationContent : IGameMessageContent
    {   
        public byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

        public void FromByteArray(byte[] bytes, int index = 38)
        {
            throw new NotImplementedException();
        }
    }
    public class InitializeContent : IGameMessageContent
    {
        private int[] _feeBoostedTiles =
        {
            1, 9, 30, 28
        };

        public int NumberOfPlayers { get; set; }
        public List<StageUnitInfo> Units { get; set; }


        public int[] FeeBoostedTiles {get { return _feeBoostedTiles; }set { _feeBoostedTiles = value; }}
        
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[(_feeBoostedTiles.Length + (NumberOfPlayers*4) +1) * sizeof(int) ];
            int index = 0;
            foreach (int feeBoostedTile in FeeBoostedTiles)
            {
                BitConverter.GetBytes(feeBoostedTile).CopyTo(bytes, index);
                index += sizeof(int);
            }

            BitConverter.GetBytes(NumberOfPlayers).CopyTo(bytes, index);
            index += sizeof(int);

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                BitConverter.GetBytes(Units[i].gold).CopyTo(bytes, index);
                index += sizeof(int);
                BitConverter.GetBytes(Units[i].Order).CopyTo(bytes, index);
                index += sizeof(int);
                BitConverter.GetBytes(Units[i].Capital).CopyTo(bytes, index);
                index += sizeof(int);
                BitConverter.GetBytes((int)Units[i].teamColor).CopyTo(bytes, index);
                index += sizeof(int);
            }
            return bytes;
        }

        public void FromByteArray(byte[] bytes, int index = 38)
        {
            _feeBoostedTiles = new int[4];
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[0]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[1]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[2]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _feeBoostedTiles[3]);

            NumberOfPlayers = BitConverter.ToInt32(bytes, index);
            index += sizeof (Int32);

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                Units.Add(new StageUnitInfo(StageUnitInfo.TEAM_COLOR.BLUE, 0));
                Units[i].gold = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                Units[i].Order = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                Units[i].Capital  = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                Units[i].teamColor = (StageUnitInfo.TEAM_COLOR)BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
            }

        }
    }

    public class RollMoveDiceResultContent : IGameMessageContent
    {
        private bool _doublePip;
        public bool DoublePip {get { return _doublePip; } set { _doublePip = value; }}
        private char[] _dices;
        public char[] Dices
        {
            get { return _dices; }
        }

        public RollMoveDiceResultContent(int[] dices)
        {
            _dices = new char[2];
            _dices[0] = (char) dices[0];
            _dices[1] = (char) dices[1];
            if (dices[0] == dices[1]) DoublePip = true;
        }

        public RollMoveDiceResultContent(byte[] bytes)
        {
            FromByteArray(bytes);
        }

        public RollMoveDiceResultContent()
        {
        }

        private const int ByteLength = sizeof(char) * 2 + sizeof(bool);
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[ByteLength];
            BitConverter.GetBytes(_dices[0]).CopyTo(bytes,0);
            BitConverter.GetBytes(_dices[1]).CopyTo(bytes,1);
            BitConverter.GetBytes(DoublePip).CopyTo(bytes,2);
            return bytes;
        }

        public void FromByteArray(byte[] bytes, int index = 38)
        {
            _dices = new char[2];
            BitConvertUtils.ReadBytes(bytes, ref index, ref _dices[0]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _dices[1]);
            BitConvertUtils.ReadBytes(bytes, ref index, ref _doublePip);
        }
    }

    public class InitUserContent : IGameMessageContent
    {
        public byte[] ToByteArray()
        {
            return new byte[0];
        }

        public void FromByteArray(byte[] bytes, int index = 38)
        {
        }
    }

    //TODO 지금은 4명으로 고정
    public class SelectOrderCardContent : IGameMessageContent
    {
        public List<int> OrderSelectCards { get; set; }
        public int SelectedCard { get; set; }
        public bool Result { get; set; }

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[4*sizeof (Int32)];
            int index = 0;
            foreach (int orderSelectCard in OrderSelectCards)
            {
                BitConverter.GetBytes(orderSelectCard).CopyTo(bytes, index);
                index += sizeof (Int32);
            }

            BitConverter.GetBytes(SelectedCard).CopyTo(bytes, index);
            index += sizeof(Int32);
            BitConverter.GetBytes(Result).CopyTo(bytes, index);

            return bytes;
        }

        public void FromByteArray(byte[] bytes, int index = 38)
        {
            for (int i = 0; i < 4; i++)
            {
                OrderSelectCards[i] = BitConverter.ToInt32(bytes, index);
                index += sizeof(Int32);
            }

            SelectedCard = BitConverter.ToInt32(bytes, index);
            index += sizeof(Int32);

             Result = BitConverter.ToBoolean(bytes, index);
        }
    }
}