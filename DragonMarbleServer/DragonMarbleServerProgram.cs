using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Dragon;
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

        private static void Main(string[] args)
        {
            InitGame();

            RajaProvider rajaProvider = new RajaProvider();

            var server = new NetworkManager(
                MaxConnection, BufferSize, QueueNumber,
                new IPEndPoint(IPAddress.Any, Port))
            {
                RajaProvider = rajaProvider
            };

            server.OnAfterAccept += DoorMan.AddPlayer;
            
            server.Start();

            Console.ReadKey();
        }

        


        private static void InitGame()
        {
            XmlConfigurator.Configure(new FileInfo("log4net.xml"));

            List<StageTileInfo> tiles = XmlUtils.LoadXml(@"data_stage.xml", GameMaster.ParseTiles);

            Logger.Debug("Start app.");

            //gm = new GameMaster(tiles);

           
        }
    }

    public class DoorMan
    {
        public static void AddPlayer(object sender, SocketAsyncEventArgs e)
        {
            IRaja token = (IRaja) e.UserToken;
            InitializePlayerGameMessage m 
                = (InitializePlayerGameMessage) GameMessageFactory.GetGameMessage(GameMessageType.InitializePlayer);
            m.PlayerId = Guid.NewGuid();
            m.Server = Guid.NewGuid();
            token.WriteArgs.SetBuffer(m.ToByteArray(), 0, m.Length);
            token.Socket.SendAsync(token.WriteArgs);
        }
    }

    internal class RajaProvider : IRajaProvider
    {
        public IRaja NewInstance()
        {
            return new Raja();
        }
    }
}