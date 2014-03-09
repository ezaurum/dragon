using System;
using System.Net.Sockets;
using Dragon;

namespace Client.Test
{
    public static class ClientTestProgram
    {
        static void Main(string[] args)
        {
            var c = new ClientDragonSocket<SimpleMessage>(new SimpleMessageFactory(), new SimpleMessage());

            c.ConnectSuccess += (sender, eventArgs) => Console.WriteLine("Connected"); 

            c.OnReadCompleted += (message, i) => Console.WriteLine("Read " +message);

            c.Disconnected += Disconnected;

            c.UpdateMessage += message => { };

            c.Connect("127.0.0.1", 10008); 
            
            Console.ReadKey();
            c.Disconnect();
            Console.WriteLine("We're "+c.State);
            Console.ReadKey();
            Console.WriteLine("We're connecting.........");
            c.Connect("127.0.0.1", 10008); 
            Console.ReadKey(); 
        }

        private static void Disconnected(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Console.WriteLine("Disconnected");
        }
    }

    
}
