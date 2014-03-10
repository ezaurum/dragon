using System;
using System.Net;
using System.Threading;
using Client.Test;
using Dragon;
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
            var s = new SocketDistributor<SimpleMessage>
            {
                Backlog = 20,
                MaximumConnection = 5,
                IpEndpoint = new IPEndPoint(IPAddress.Any, 10008),
                MessageFactory = new SimpleMessageFactory()

            };
            s.Accepted += (sender, eventArgs) =>
            {
                //Something to test
                var userToken = (ServerDragonSocket<SimpleMessage>) eventArgs.UserToken;

                userToken.OnReadCompleted += (message, i) => Console.WriteLine("READ " + message);
                userToken.Disconnected += (o, asyncEventArgs) => Console.WriteLine("deiscon");
                userToken.HeartbeatEnable = true;
                
                userToken.HeartBeatReceiver = new HeartBeatReceiver<SimpleMessage>()
                {
                    IsHeartBeat = IsHeartBeat
                };

                userToken.ReceiveHeartbeat += (message, i) => userToken.Send(message);

                userToken.Activate();
            };

            Thread.Sleep(1000);
            s.Start();
            Console.ReadKey();
        }

        private static bool IsHeartBeat(SimpleMessage arg)
        {
            Console.WriteLine("BEAT");
            return true;
        }
    }

}
