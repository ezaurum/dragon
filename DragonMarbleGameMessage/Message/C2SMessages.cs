using System;

namespace DragonMarble.Message
{
    public class RollMoveDiceContentC2S : IGameMessageContent
    {
        public RollMoveDiceContentC2S(byte[] bytes)
        {
            FromByteArray(bytes);
        }

        public RollMoveDiceContentC2S()
        {
            
        }

        public int Pressed { get; set; }

        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(Pressed);
        }

        public void FromByteArray(byte[] bytes, int index = 38)
        {
            Pressed = BitConverter.ToInt32(bytes, index);
        }
    }

    public class InitializeContentC2S : IGameMessageContent
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
}