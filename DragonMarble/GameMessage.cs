namespace DragonMarble
{
    public struct GameMessage
    {
        public string Guid;
        public long Timestamp;
        public string PublicKey;
        public string SessionKey;
        public byte[] Contents;
    }
}