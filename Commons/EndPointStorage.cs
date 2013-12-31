using System.Net;

namespace Dragon
{
    public static class EndPointStorage
    {
        public static readonly IPEndPoint DefaultDestination = new IPEndPoint(IPAddress.Loopback, 10008);
        public static readonly IPEndPoint DefaultAcceptable = new IPEndPoint(IPAddress.Any, 10008);
    }
}