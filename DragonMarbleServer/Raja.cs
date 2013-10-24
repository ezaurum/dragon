using System.Net.Sockets;
using Dragon;

namespace DragonMarble
{
    public partial class Raja : IRaja
    {
        private StageUnitInfo _unit;
        public StageUnitInfo Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                _unit = value;
                _unit.MessageProcessor = this;
            } 
        }

        public INetworkManager NetworkManager { get; set; }
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadArgs { get; set; }
        public SocketAsyncEventArgs WriteArgs { get; set; }
        public bool IsDisposed { get; set; }


        public void Dispose()
        {
            Socket = null;
            ReadArgs.UserToken = null;
            ReadArgs = null;
            WriteArgs.UserToken = null;
            WriteArgs = null;
            IsDisposed = true;
        }
    }
}