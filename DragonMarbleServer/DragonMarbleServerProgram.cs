using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Dragon;
using Dragon.Server;
using GameUtils;
using log4net;
using log4net.Config;

using Microsoft.VisualStudio.Profiler;

namespace DragonMarble
{
    public class DragonMarbleServerProgram
    {
        private const int Port = 10008;
        private const int QueueNumber = 8000;
        private const int BufferSize = 1024;
        private const int MaxConnection = 16000;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DragonMarbleServerProgram));

        private static void Main(string[] args)
        {
            DataCollection.StopProfile(
                ProfileLevel.Global,
                DataCollection.CurrentId);
            
            XmlConfigurator.Configure(new FileInfo("log4net.xml"));
            Logger.Debug("Start app.");

            InitGameData();

            RajaProvider rajaProvider = new RajaProvider();

            var server = new NetworkManager(
                MaxConnection, BufferSize, QueueNumber,
                new IPEndPoint(IPAddress.Any, Port))
            {
                RajaProvider = rajaProvider
            };
            
            GameMasterPool pool =new GameMasterPool();
            ISessionManager sessionManager = new SessionManager();
            server.OnAfterAccept += sessionManager.Login;
            
            //server.OnAfterAccept += pool.AddPlayer;

            server.Start();

            Running = true;

            while (Running)
            {
                string readLine = Console.ReadLine();
                
                //dice cheat
                int i;
                if (int.TryParse(readLine, out i))
                {
                    if (i > 1 && i < 13) StageDiceInfo.diceCheat = i;
                }

                if (readLine.Contains("A") || readLine.Contains("a"))
                {
                    StageUnitInfo s = GetNewAI();

                    if (null != GameMaster.Temp)
                    {
                        GameMaster.Temp.Join(s);
                    }
                    continue;
                }

                if (readLine.Contains("Q") || readLine.Contains("q")) Running = false;
            }

            DataCollection.StopProfile(
                ProfileLevel.Global,
                DataCollection.CurrentId);
        }

        private static AIStageUnitInfo GetNewAI()
        {
            return new AIStageUnitInfo
            {
                Id = Guid.NewGuid(),
                UnitColor = StageUnitInfo.UNIT_COLOR.GREEN,
                Gold = 2000000,
                ControlMode = StageUnitInfo.ControlModeType.AI_0,
                CharacterId = 1,
            };
        }

        public static bool Running { get; set; }

        private static void InitGameData()
        {
            List<StageTileInfo> tiles = XmlUtils.LoadXml(@"data_stage.xml", GameMaster.ParseTiles);
            GameMaster.ChanceCardList = XmlUtils.LoadXml(@"data_chance.xml", GameMaster.ParseCards);
            GameMaster.OriginalBoard = new GameBoard(tiles);
            
        }
    }

    

    internal class GameMasterPool
    {
        GameMaster gm = new GameMaster(0, GamePlayType.Individual2PlayerPlay);

        public GameMaster GetGameMaster()
        {
            return gm;
        }

        /// <summary>
        /// after Connect event
        /// </summary>
        /// <param name="sender"></param> 
        /// <param name="e"></param>
        public void AddPlayer(object sender, SocketAsyncEventArgs e)
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
            
            Join(token.Unit);
            
        }

        public void Join(StageUnitInfo unit)
        {
            GetGameMaster().Join(unit);
        }
    }
}