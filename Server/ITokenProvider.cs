using System.Net.Sockets;

namespace Dragon.Server
{
    public interface ITokenProvider
    {
        IAsyncUserToken NewAsyncUserToken();
    }

    public interface IAsyncUserToken
    {
        Socket Socket { get; set; }
        SocketAsyncEventArgs ReadArg { get; set; }
        SocketAsyncEventArgs WriteArg { get; set; }
    }
}