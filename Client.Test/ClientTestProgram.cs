using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Dragon;

namespace Client.Test
{
    public static class ClientTestProgram
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(Test);
            Task.Factory.StartNew(Test);
            Task.Factory.StartNew(Test);
            Task.Factory.StartNew(Test);

            Console.ReadKey();
        }

        private static void Test()
        {
            var c = new ClientDragonSocket<SimpleMessage>(new SimpleMessageFactory(),
                new SimpleMessage());

            c.ConnectSuccess += (sender, eventArgs) => Console.WriteLine("Connected");

            c.OnReadCompleted += (message, i) => Console.WriteLine("Read " + message);

            c.Disconnected += Disconnected;

            c.UpdateMessage += message => { };

            c.Connect("127.0.0.1", 10008);
        }

        private static void Disconnected(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Console.WriteLine("Disconnected");
        }
    }

    
}
