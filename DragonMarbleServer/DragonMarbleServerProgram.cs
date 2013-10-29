using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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

            server.OnAfterAccept += GameMaster.AddPlayer;
            
            server.Start();
            string readLine = "";

            while (true)
            {
                readLine = Console.ReadLine();
                int i;
                if (int.TryParse(readLine, out i))
                {
                    if (i > 1 && i < 13) StageDiceInfo.diceCheat = i;
                }

                if (readLine.Contains("Q") || readLine.Contains("q")) return;
            }
            
        }
    }
}