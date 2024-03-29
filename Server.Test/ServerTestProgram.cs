﻿using System;
using System.Collections.Generic;
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

            var ss = new List<ServerDragonSocket<SimpleMessage>>();
 
            var s = new SocketDistributor<SimpleMessage>
            {
                Backlog = 1000,
                IpEndpoint = new IPEndPoint(IPAddress.Any, 20009),
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

                userToken.ReadCompleted += (message, i) =>
                {
                    Console.WriteLine(userToken.RemoteEndPoint + ":" + message.BoardType + " - " + _index);
                    message.PlayMode = (byte) Interlocked.Increment(ref _index);
                    Interlocked.Increment(ref _sendIndex);
                    userToken.Send(message);
                };
                userToken.Disconnected += (o, asyncEventArgs) =>
                {
                    Interlocked.Decrement(ref connection);

                    Console.WriteLine("discon " + connection);
                    
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

                userToken.Activate();
                
                Interlocked.Increment(ref connection);
                Console.WriteLine("con " + connection);
                
                ss.Add(userToken);
            };

            Thread.Sleep(1000);
            s.Start();
            t.Start();

            Console.ReadKey();

            ServerDragonSocket<SimpleMessage>[] serverDragonSockets = ss.ToArray();
            foreach (ServerDragonSocket<SimpleMessage> dragonSocket in serverDragonSockets)
            {
                Console.WriteLine("dd");
                dragonSocket.Disconnect();
                ss.Remove(dragonSocket);
            }

            Console.ReadKey();

            serverDragonSockets = ss.ToArray();
            foreach (ServerDragonSocket<SimpleMessage> dragonSocket in serverDragonSockets)
            {
                Console.WriteLine("dd");
                dragonSocket.Disconnect();
                ss.Remove(dragonSocket);
            }

            Console.ReadKey();

            serverDragonSockets = ss.ToArray();
            foreach (ServerDragonSocket<SimpleMessage> dragonSocket in serverDragonSockets)
            {
                Console.WriteLine("dd");
                dragonSocket.Disconnect();
                ss.Remove(dragonSocket);
            }

            Console.ReadKey();

            serverDragonSockets = ss.ToArray();
            foreach (ServerDragonSocket<SimpleMessage> dragonSocket in serverDragonSockets)
            {
                Console.WriteLine("dd");
                dragonSocket.Disconnect();
                ss.Remove(dragonSocket);
            }

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
