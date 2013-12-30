using System.Net;

namespace Dragon
{
    public abstract class EndPointStorage
    {
        protected static readonly IPEndPoint DefaultDestination = new IPEndPoint(IPAddress.Loopback, 10008);
        protected static readonly IPEndPoint DefaultAcceptable = new IPEndPoint(IPAddress.Any, 10008);
    }
}