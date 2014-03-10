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
        private static int _index;

        static void Main(string[] args)
        {
            BasicConfigurator.Configure();

            var s = new SocketDistributor<SimpleMessage>
            {
                Backlog = 20,
                MaximumConnection = 5,
                IpEndpoint = new IPEndPoint(IPAddress.Any, 10008),
                MessageFactoryProvide = MessageFactoryProvide

            };
            s.Accepted += (sender, eventArgs) =>
            {
                //Something to test
                var userToken = (ServerDragonSocket<SimpleMessage>) eventArgs.UserToken;

                userToken.OnReadCompleted += (message, i) =>
                {
                    Console.WriteLine(userToken.RemoteEndPoint + ":" + message.BoardType + " - " + message.PlayMode);
                    message.PlayMode = (byte) Interlocked.Increment(ref _index);
                    userToken.Send(message);
                };
                userToken.Disconnected += (o, asyncEventArgs) => Console.WriteLine("deiscon");
                userToken.HeartbeatEnable = true;
                
                userToken.HeartBeatReceiver = new HeartBeatReceiver<SimpleMessage>
                {
                    IsHeartBeat = IsHeartBeat
                };

                userToken.ReceiveHeartbeat += (message, i) =>
                {
                    message.PlayMode = (byte) Interlocked.Increment(ref _index);
                    userToken.Send(message);
                };

                userToken.Activate();
            };

            Thread.Sleep(1000);
            s.Start();
            Console.ReadKey();
        }

        private static MessageConverter<SimpleMessage, SimpleMessage> MessageFactoryProvide()
        {
            byte[] buffer = new byte[1024];
            return new MessageConverter<SimpleMessage, SimpleMessage>(buffer,0, 1024, new SimpleMessageFactory());
        }

        private static bool IsHeartBeat(SimpleMessage arg)
        {
            return false;
        }
    }

}
