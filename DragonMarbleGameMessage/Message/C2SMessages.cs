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
    }
}