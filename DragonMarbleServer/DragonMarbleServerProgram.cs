using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Dragon.Server;
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
            Logger.Debug("Start app.");

            gm = new GameMaster();

            List<StageTileInfo> tiles = XmlUtils.LoadXml(@"data_stage.xml", GameMaster.ParseTiles);

            gm.Board = new GameBoard(tiles);
            
            RajaProvider rajaProvider = new RajaProvider();

            var server = new NetworkManager(
                MaxConnection, BufferSize, QueueNumber,
                new IPEndPoint(IPAddress.Any, Port))
            {
                RajaProvider = rajaProvider
            };

            server.OnAfterAccept += AddPlayer;
            
            server.Start();

            Console.ReadKey();
        }


        public static void AddPlayer(object sender, SocketAsyncEventArgs e)
        {
            Raja token = (Raja)e.UserToken;
            token.Unit = new StageUnitInfo
            {
                Id = Guid.NewGuid(),
                Order = 0,
                UnitColor = StageUnitInfo.UNIT_COLOR.BLUE,
                CharacterId = 1,
                Gold = 2000000
            };

            gm.Join(token.Unit);

            if (gm.IsGameStartable)
            {
                gm.StartGame();
            }
        }

    }
}