using System;

namespace DragonMarble.Message
{
    public class RollMoveDiceContentC2S : IGameMessageContent
    {
        public int Pressed { get; set; }

        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(Pressed);
        }

        public void FromByteArray(byte[] bytes, int index = 0)
        {
            Pressed = BitConverter.ToInt32(bytes, index);
        }
    }
}