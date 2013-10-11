using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using Dragon.Server;
using DragonMarble.Message;
using GameUtils;
using log4net;
using log4net.Config;

namespace DragonMarble
{
    public class DragonMarbleServerProgram
    {
        private const int Port = 10008;
        private const int QueueNumber = 1000;
        private const int BufferSize = 1024;
        private const int MaxConnection = 3000;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DragonMarbleServerProgram));

        private static GameMaster gm;

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.xml"));

            IList<StageTile> tiles = XmlUtils.LoadXml(@"data_stage.xml", StageTile.Parse);

            Logger.Debug("Start app.");

            gm = new GameMaster(tiles);

            var server = new MultiClientServer(
                MaxConnection, BufferSize, QueueNumber,
                new IPEndPoint(IPAddress.Any, Port), new MessageProcessorProvier());
            server.OnReceiveBytes += ConvertBytesToMessage;
            server.OnAcceptConnection += AddPlayer;
            server.Start();

            Console.ReadKey();
        }

        private static void AddPlayer(object sender, SocketAsyncEventArgs eventArgs)
        {   
            Logger.Debug("connectecd.");
            QueuedMessageProcessor token = (QueuedMessageProcessor)eventArgs.UserToken;

            GameMessage idMessage = new GameMessage
            {
                Header = new GameMessageHeader
                {
                    From = Guid.NewGuid(),
                    To = Guid.NewGuid()
                },
                Body = new GameMessageBody
                {
                    MessageType = GameMessageType.InitUser,
                    Content = new InitUserContent()
                }
                
            };

            token.SendingMessage = idMessage;

        }

        private static void ConvertBytesToMessage(object sender, SocketAsyncEventArgs eventArgs)
        {
            QueuedMessageProcessor token = (QueuedMessageProcessor) eventArgs.UserToken;
            short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
            byte[] m = eventArgs.Buffer.Skip(eventArgs.Offset).Take(messageLength).ToArray();
            GameMessage gameMessage = GameMessage.FromByteArray(m, GameMessageFlowType.C2S);
            Logger.DebugFormat("receivec. {0}", gameMessage.MessageType);
            token.ReceivedMessage = gameMessage;
        }
    }

}