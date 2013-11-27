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
            socketAsyncEventArgs.Completed += MakeSession;
            
            SocketConnector c = new SocketConnector
            {
                ConnectEventArgs = socketAsyncEventArgs
            };

            c.Connect("127.0.0.1",10008);
            
            Console.ReadKey();
        }

        private static void MakeSession(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;

            SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs();
            readEventArgs.Completed += (o, args) =>
            {
                if (args.BytesTransferred > 0 
                    && args.SocketError == SocketError.Success)
                {
                    ((Socket)o).ReceiveAsync(args);
                }
            };
            
            SocketAsyncEventArgs writeEventArgs = new SocketAsyncEventArgs();
            writeEventArgs.Completed += (o, args) =>
            {
                if (args.SocketError == SocketError.Success)
                {
                    
                }
            };
            
            DummySession ds = new DummySession
            {
                Socket = e.AcceptSocket,
                ReadEventArgs = readEventArgs,
                WriteEventArgs = writeEventArgs
            };

            //start read
        }
    }

    public class DummySession : ISession<DummyMessage>
    {
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadEventArgs { get; set; }
        public SocketAsyncEventArgs WriteEventArgs { get; set; }
        public IMessageProcessor<DummyMessage> MessageProcessor { get; set; }
        public event EventHandler<MessageAsyncEventArgs<DummyMessage>> OnMessageSent;
        public event EventHandler<MessageAsyncEventArgs<DummyMessage>> OnMessageConverted;
        public event EventHandler<SocketAsyncEventArgs> OnSendCompleted;
        public event EventHandler<SocketAsyncEventArgs> OnReceiveCompleted;

        



        public void StartRead()
        {
            if (!Socket.ReceiveAsync(ReadEventArgs))
            {
                OnReceiveCompleted(Socket, ReadEventArgs);
            }
        }
    }

    public class SimpleSession<T> : ISession<T> where T : IMessage
    {
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs ReadEventArgs { get; set; }
        public SocketAsyncEventArgs WriteEventArgs { get; set; }
        public IMessageProcessor<T> MessageProcessor { get; set; }
        public event EventHandler<MessageAsyncEventArgs<T>> OnMessageSent;
        public event EventHandler<MessageAsyncEventArgs<T>> OnMessageConverted;
        public event EventHandler<SocketAsyncEventArgs> OnSendCompleted;
        public event EventHandler<SocketAsyncEventArgs> OnReceiveCompleted;

        public SimpleSession(SocketAsyncEventArgs readArgs, SocketAsyncEventArgs writeArgs)
        {
            ReadEventArgs = readArgs;
            WriteEventArgs = writeArgs;
            OnReceiveCompleted += ReadCheck;
            ReadEventArgs.Completed += OnReceiveCompleted;
        }

        private void ReadCheck(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 
                && args.SocketError == SocketError.Success)
            {
                ReadAsyncContinually();

                MessageAsyncEventArgs<T> messageAsyncEventArgs = new MessageAsyncEventArgs<T>();
                //TODO some converting method
                OnMessageConverted(this, messageAsyncEventArgs);
            }
        }

        public void ReadAsyncContinually()
        {
            if (!Socket.ReceiveAsync(ReadEventArgs))
            {
                OnReceiveCompleted(Socket, ReadEventArgs);
            }
        }
    }

    public class DummyMessage : IMessage
    {
        public short Length
        {
            get { return 8; }
        }

        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(Int64.MaxValue);
        }

        public void FromByteArray(byte[] bytes)
        {
            
        }

        public DateTime PacketTime { get; set; }
    }

    /*public class DummyClientActionController
    {
        public void Init(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
             Console.WriteLine("Init");
        }
    }

    public class DummyClientSessionManager
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

    public class DummyClientAuthorizationManager
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
    }*/
}
