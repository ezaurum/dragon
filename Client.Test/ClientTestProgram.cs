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
            am.Authorized += sm.RequestSession;
            sm.SessionAcquired += ac.Init;
            
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
            CheckSuccess(sender, e);
        }

        public event EventHandler<SocketAsyncEventArgs> SessionAcquired;
        
        private void CheckSuccess(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("check success");
            SessionAcquired(sender, e);
        }
    }

    public class DummyClientAuthorizationManager : IAuthorizationManager
    {
        public void Login(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            Console.WriteLine("login");
            CheckSuccess(sender, e);
            
        }

        private void CheckSuccess(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("check success");
            Authorized(sender, e);
        }

        public event EventHandler<SocketAsyncEventArgs> Authorized;
    }
}
