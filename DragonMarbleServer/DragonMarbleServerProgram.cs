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

        private static GameMaster gm;

        private static void Main(string[] args)
        {
            InitGame();

            TokenProvider tokenProvider = new TokenProvider();
            tokenProvider.MessageProcess = new MessageProcessorProvier<IDragonMarbleGameMessage>
            {
                MessageFactoryMethod = GameMessageFactory.GetGameMessage
            };

            var server = new NetworkManager(
                MaxConnection, BufferSize, QueueNumber,
                new IPEndPoint(IPAddress.Any, Port))
            {
                TokenProvider = tokenProvider
            };
            server.OnAfterAccept += AddPlayer;
            
            server.Start();

            Console.ReadKey();
        }

        private static void AddPlayer(object sender, SocketAsyncEventArgs eventArgs)
        {
            Logger.Debug("connectecd.");
            QueuedMessageProcessor<IDragonMarbleGameMessage> token = (QueuedMessageProcessor<IDragonMarbleGameMessage>)eventArgs.UserToken;
            
            StageUnitInfo player = new StageUnitInfo
            {
                Id = Guid.NewGuid(),
                MessageProcessor = token,
                Order = 0,
                UnitColor = StageUnitInfo.UNIT_COLOR.BLUE,
                CharacterId = 1,
                Gold = 2000000
            };
            
         /*  gm.Join(player);

            if (gm.IsGameStartable)
            {
                gm.StartGame();
            }*/
        }

        private static void InitGame()
        {
            XmlConfigurator.Configure(new FileInfo("log4net.xml"));

            List<StageTileInfo> tiles = XmlUtils.LoadXml(@"data_stage.xml", GameMaster.ParseTiles);

            Logger.Debug("Start app.");

            gm = new GameMaster(tiles);

           
        }
    }

    internal class TokenProvider : ITokenProvider
    {
        public IAsyncUserToken NewAsyncUserToken()
        {
            return new AsyncUserToken();
        }

        public MessageProcessorProvier<IDragonMarbleGameMessage> MessageProcess { get; set; }
    }
}