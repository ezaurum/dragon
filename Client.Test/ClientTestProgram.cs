using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Dragon;
using Timer = System.Timers.Timer;

namespace Client.Test
{
    public static class ClientTestProgram
    {
        private static int _index;
        private static int _sendIndex;
        private static int _receiveIndex;

        static void Main(string[] args)
        {
            for (int i = 0; i < 1; i++)
            {
                Task.Factory.StartNew(Test);
                Task.Factory.StartNew(Test);
            }

            Console.ReadKey();

            foreach (ConcurrentClientDragonSocket<SimpleMessage, SimpleMessage> socket in a)
            {
                socket.Disconnect();
            }

            Console.ReadKey();
        }

        private static Stack<ConcurrentClientDragonSocket<SimpleMessage, SimpleMessage>> a = new Stack<ConcurrentClientDragonSocket<SimpleMessage, SimpleMessage>>();

        private static void Test()
        {
            byte[] buf = new byte[1024];

            ClientDragonSocket<SimpleMessage> d = null;
            try
            {
                d =
                    new ClientDragonSocket<SimpleMessage>(
                        new MessageConverter<SimpleMessage, SimpleMessage>(buf,
                            0, 1024, new SimpleMessageFactory()));
                d.ConnectSuccess +=
                (sender, args) => Console.WriteLine("connected.  sucessssss");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            byte[] buffer = new byte[1024];

            var c = new ConcurrentClientDragonSocket<SimpleMessage, SimpleMessage>
                (new MessageConverter<SimpleMessage, SimpleMessage>
                    (buffer, 0, 1024,new SimpleMessageFactory()), 
                    new SimpleMessage
            {
                BoardType = 32,
                PlayMode = 53,
                PlayType = 'A',
            });

            c.ConnectSuccess += (sender, eventArgs) => Console.WriteLine("Connected");

            c.ReadCompleted += (message, code) => Console.WriteLine(c.LocalEndPoint + "[" +message.PlayMode);

            c.Disconnected += Disconnected;
            
            Timer t = new Timer
            {
                Interval = 1000
            };

            t.Elapsed += (o, elapsedEventArgs) =>
            {
                SimpleMessage message = new SimpleMessage
                {
                    
                };
                c.Send(message);
            };
            try
            {
                Console.WriteLine("connect?>");
//                c.Connect("localhost", 20009);

//                c.Connect("127.0.0.1", 20009);
                d.Connect("127.0.0.1", 20009);

            }
            catch (Exception e)
            {
                Console.WriteLine("WTF??????????" +e);
            }

            t.Start();

            }

        private static void Disconnected(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Console.WriteLine("Disconnected p");
        }
    }

    
}
