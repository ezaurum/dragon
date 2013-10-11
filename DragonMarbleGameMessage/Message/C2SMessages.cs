using System;

namespace DragonMarble.Message
{
    public class RollMoveDiceContent : IGameMessageContent
    {
        public RollMoveDiceContent(byte[] bytes)
        {
            FromByteArray(bytes);
        }

        public RollMoveDiceContent()
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
}