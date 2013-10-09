using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Dragon.Server;
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
            server.Start();

            Console.ReadKey();
        }
    }
}