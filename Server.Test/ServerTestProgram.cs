using System;
using System.Net;
using System.Threading;
using Client.Test;
using Dragon;
using log4net.Config;
using Timer = System.Timers.Timer;


namespace Server.Test
{
    /// <summary>
    /// .Test for socket distributor
    /// </summary>
    public static class ServerTestProgram
    {
        private static int _index;
        private static int _sendIndex;


        static void Main(string[] args)
        {
            int connection = 0;

            BasicConfigurator.Configure();

            Random random = new Random();

            var s = new SocketDistributor<SimpleMessage>
            {
                Backlog = 4000,
                MaximumConnection = 4001,
                IpEndpoint = new IPEndPoint(IPAddress.Any, 10008),
                MessageFactoryProvide = MessageFactoryProvide

            };

            Timer t = new Timer
            {
                Interval = 1000
            };

            s.Accepted += (sender, eventArgs) =>
            {
                //Something to test
                var userToken = (ServerDragonSocket<SimpleMessage>) eventArgs.UserToken;

                userToken.OnReadCompleted += (message, i) =>
                {
                    //Console.WriteLine(userToken.RemoteEndPoint + ":" + message.BoardType + " - " + message.PlayMode);
                    message.PlayMode = (byte) Interlocked.Increment(ref _index);
                    Interlocked.Increment(ref _sendIndex);
                    userToken.Send(message);
                };
                userToken.Disconnected += (o, asyncEventArgs) =>
                {

                    Interlocked.Decrement(ref connection);

                    Console.WriteLine("deiscon " + connection);
                };
                userToken.HeartbeatEnable = true;
                
                userToken.HeartBeatReceiver = new HeartBeatReceiver<SimpleMessage>
                {
                    IsHeartBeat = IsHeartBeat
                };

                userToken.ReceiveHeartbeat += (message, i) =>
                {
                    message.PlayMode = (byte) Interlocked.Increment(ref _index);
                    Interlocked.Increment(ref _sendIndex);
                    userToken.Send(message);
                };

                t.Elapsed += (o, elapsedEventArgs) =>
                {
                    SimpleMessage message = new SimpleMessage
                    {
                        PlayType = (char) random.Next(),
                        BoardType = (byte) random.Next(),
                        PlayMode = (byte) random.Next(),
                        PacketTime = DateTime.Now
                    };
                    userToken.Send( message);
                    //Console.WriteLine(message);
                };

                userToken.Activate();
                
                Interlocked.Increment(ref connection);
                Console.WriteLine("con " + connection);
            };

            Thread.Sleep(1000);
            s.Start();
            t.Start();

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
