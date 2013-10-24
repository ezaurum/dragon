using System.Net.Sockets;
using Dragon;

namespace DragonMarble
{
    public partial class Raja : IRaja
    {
        public Raja()
        {
            Unit = new StageUnitInfo {MessageProcessor = this};
        }

        public StageUnitInfo Unit { get; set; }

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