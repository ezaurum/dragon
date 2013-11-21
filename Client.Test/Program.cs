using System;
using System.Threading;
using Dragon.Client;

namespace Client.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            SocketConnector c = new SocketConnector();
            c.Connect();
            Console.ReadKey();
        }
    }
}
