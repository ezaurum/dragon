using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dragon;

namespace Client.Test
{
    public static class ClientTestProgram
    {
        private static int _index;
        private static int _sendIndex;
        private static int _receiveIndex;

        static void Main(string[] args)
        {
            for (int i = 0; i < 4000; i++)
            {
                Task.Factory.StartNew(Test);
                Task.Factory.StartNew(Test);
            }
            
            /*Task.Factory.StartNew(Test);
            Task.Factory.StartNew(Test);
            Task.Factory.StartNew(Test);*/

            Console.ReadKey();
        }

        private static void Test()
        {
            byte[] buffer = new byte[1024];

            var c = new ConcurrentClientDragonSocket<SimpleMessage, SimpleMessage>(new MessageConverter<SimpleMessage, SimpleMessage>(buffer, 0, 1024,new SimpleMessageFactory()),
                new SimpleMessage
                {
                    PlayType = (char)new Random(Interlocked.Increment(ref _index)).Next()
                }, 333);

            c.ConnectSuccess += (sender, eventArgs) => Console.WriteLine("Connected");

            c.OnReadCompleted += (message, code) => Console.WriteLine(c.LocalEndPoint + "[" +
                                                                                        Interlocked.Increment(ref _receiveIndex) +
                                                                                        "] :" + message.BoardType + " - " +
                                                                                        message.PlayMode);

            c.Disconnected += Disconnected;

            c.UpdateMessage += message =>
            {
                message.BoardType = (byte)Interlocked.Increment(ref _index);
            };

            c.Connect("127.0.0.1", 10008);
        }

        private static void Disconnected(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Console.WriteLine("Disconnected");
        }
    }

    
}
