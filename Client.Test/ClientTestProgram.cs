using System;
using System.Net.Sockets;
using Dragon;
using Dragon.Client;

namespace Client.Test
{
    public static class ClientTestProgram
    {
        static void Main(string[] args)
        {
            SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
            IAuthorizationManager am = new DummyClientAuthorizationManager();
            ISessionManager sm = new DummyClientSessionManager();
            IActionController ac = new DummyClientActionController();

            socketAsyncEventArgs.Completed += am.Login;
            socketAsyncEventArgs.Completed += sm.RequestSession;
            socketAsyncEventArgs.Completed += ac.Init;
            
            SocketConnector c = new SocketConnector
            {
                ConnectEventArgs = socketAsyncEventArgs
            };

            c.Connect("127.0.0.1",10008);
            
            Console.ReadKey();
        }
    }

    public class DummyClientActionController : IActionController
    {
        public void Init(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
             Console.WriteLine("Init");
        }
    }

    public class DummyClientSessionManager : ISessionManager
    {
        public void RequestSession(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            Console.WriteLine("request session");
        }
    }

    public class DummyClientAuthorizationManager : IAuthorizationManager
    {
        public void Login(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            Console.WriteLine("login");
        }
    }
}
