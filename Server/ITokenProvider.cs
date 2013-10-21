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
        byte[] SendingMessageByteArray();
    }
}