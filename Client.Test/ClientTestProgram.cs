using System;
using Dragon.Client;

namespace Client.Test
{
    public static class ClientTestProgram
    {
        static void Main(string[] args)
        {
            SocketConnector c = new SocketConnector();
            c.Connect("127.0.0.1",10008);
            
            Console.ReadKey();
        }
    }
}
