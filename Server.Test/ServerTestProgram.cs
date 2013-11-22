using System;
using System.Net;
using Dragon;
using Dragon.Server;
using log4net.Config;

namespace Server.Test
{
    /// <summary>
    /// .Test for socket distributor
    /// </summary>
    public static class ServerTestProgram
    {
        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            SocketAsyncEventArgsPool socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            socketAsyncEventArgsPool.Completed += (sender, e) => Console.WriteLine("Ftestsaefsd");
            SocketDistributor s = new SocketDistributor
            {
                AcceptPool = socketAsyncEventArgsPool,
                Backlog = 20,
                MaximumConnection = 5,
                IpEndpoint = new IPEndPoint(IPAddress.Any, 10008)
            };
            Console.ReadKey();
            s.Start();
            Console.ReadKey();
        }
    }
}
