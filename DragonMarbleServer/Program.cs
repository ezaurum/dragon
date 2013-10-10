using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Dragon.Interfaces;
using Dragon.Server;
using DragonMarble.Message;
using GameUtils;
using log4net;
using log4net.Config;

namespace DragonMarble
{
    public class Program
    {
        private const int Port = 10008;
        private const int QueueNumber = 1000;
        private const int BufferSize = 1024;
        private const int MaxConnection = 3000;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.xml"));

            IList<StageTile> tiles = XmlUtils.LoadXml(@"data_stage.xml", StageTile.Parse);

            Logger.Debug("Start app.");

            GameMaster gm = new GameMaster(tiles);

            var server = new MultiClientServer(
                MaxConnection, BufferSize, QueueNumber,
                new IPEndPoint(IPAddress.Any, Port), gm);
            server.OnReceiveBytes += delegate(object sender, SocketAsyncEventArgs eventArgs)
            {
                AsyncUserToken token = eventArgs.UserToken as AsyncUserToken;

                short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
                byte[] m = eventArgs.Buffer.Skip(eventArgs.Offset).Take(messageLength).ToArray();

                GameMessageType gameMessageType = (GameMessageType)BitConverter.ToInt32(m, GameMessageHeader.HeaderLength);
                IGameMessage gameMessage = new GameMessage()
                {
                    Header = new GameMessageHeader()
                    {
                        MessageLength = messageLength,
                        From = new Guid(m.Skip(GameMessageHeader.FirstGuidIndex).Take(16).ToArray()),
                        To = new Guid(m.Skip(GameMessageHeader.SecondGuidIndex).Take(16).ToArray())
                    }

                };

                if (gameMessageType == GameMessageType.Roll)
                {

                    IGameMessageBody body = new GameMessageBody()
                    {
                        MessageType = gameMessageType,
                        Content = new RollMoveDiceContentC2S()
                        {
                            Pressed = BitConverter.ToInt32(m, GameMessageHeader.HeaderLength + 4)
                        }
                    };
                    gameMessage.Body = body;
                }

                token.ReceivedMessage = gameMessage;
            };
            server.Start();

            Console.ReadKey();
        }
    }
}