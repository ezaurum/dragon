using System;
using System.Net;
using Dragon;
using Dragon.Server;
using log4net.Config;

namespace Server.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            SocketAsyncEventArgsPool socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            SocketDistributor s = new SocketDistributor
            {
                AcceptPool = socketAsyncEventArgsPool,
                Backlog = 20,
                MaximumConnection = 5,
                IpEndpoint = new IPEndPoint(IPAddress.Any, 10008)
            };
            s.Start();
            Console.ReadKey();
        }
    }
}
