using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Dragon.Client;
using DragonMarble;
using DragonMarble.Message;

namespace ConsoleTest
{
    public partial class ClientProgram
    {
        private static StageUnitInfo _unitInfo;
        private static Dictionary<Guid, StageUnitInfo> _units;
        private static Guid _server;

        private static List<bool> OrderCardSelectState { get; set; }

        private static void Main(string[] args)
        {
            ClientSessionManager sessionManager = new ClientSessionManager();

            OrderCardSelectState = new List<bool>();
            _units = new Dictionary<Guid, StageUnitInfo>();

            Unity3DNetworkManager nm = new Unity3DNetworkManager("127.0.0.1", 10008);
            nm.OnAfterMessageReceive += ProcessClientReceivedMessage;
            nm.OnAfterMessageSend += (sender, eventArgs) => Console.WriteLine("Message Sent");
            nm.OnAfterConnectOnce += sessionManager.SessionAssign;

            nm.Start();

            while (true)
            {
                string readLine = Console.ReadLine();

                if (CheckAction(readLine, nm)) return;
            }
        }

        private static void ProcessClientReceivedMessage(object o, SocketAsyncEventArgs eventArgs)
        {
            QueueAsyncClientUserToken token = (QueueAsyncClientUserToken) eventArgs.UserToken;

            Console.WriteLine("Offeset , {0}", eventArgs.Offset);
            Console.WriteLine("Buffer Length , {0}", eventArgs.Buffer.Length);

            short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
            if (messageLength < 6) return;

            byte[] m = new byte[messageLength];
            Buffer.BlockCopy(eventArgs.Buffer, eventArgs.Offset, m, 0, messageLength);
            var dragonMarbleGameMessage = GameMessageFactory.GetGameMessage(m);
            Console.WriteLine("receive , type: {0}, length :{1}", dragonMarbleGameMessage.MessageType, m.Length);
            token.Message = dragonMarbleGameMessage;

            Console.WriteLine("=======================================================================");
            SwitchMessage(dragonMarbleGameMessage);
            Console.WriteLine("=======================================================================");
            Console.WriteLine("current thread : {0}", Thread.CurrentThread.ManagedThreadId);
        }
    }

    public class ClientSessionManager
    {
        public Guid SessionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid GameRoomId { get; set; }
        public PhysicalAddress MacAddress { get; set; }

        public Unity3DNetworkManager NetworkManager { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs">connected socket args</param>
        public void SessionAssign(object sender, SocketAsyncEventArgs eventArgs)
        {
            Console.WriteLine("Session Assign");
            
            NetworkManager.SendMessage(new RequestSessionGameMessage
            {
                GameAccountId = Guid.NewGuid()
            });
            
        }
        public bool IsConnected { get; set; }
    }
}