using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

            List<StageTile> tiles = XmlUtils.LoadXml(@"data_stage.xml", StageTile.Parse);

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

            //initialize player
            InitializePlayer(token);

            if (gm.IsGameStartable)
            {
                gm.StartGame();
                
            }
        }

        private static void InitializePlayer(QueuedMessageProcessor token)
        {
            GamePlayer player = new GamePlayer {Id = Guid.NewGuid()};
            
            //set initailize player message

            InitailizePlayerGameMessage idMessage = new InitailizePlayerGameMessage
            {
                To = player.Id,
                From = Guid.NewGuid()
            };

            token.SendingMessage = idMessage;
            //player is just object
            //TODO something should be done.
            token.Player = player;
            player.Token = token;
            
            gm.Join(player);

            GamePlayer player0 = new AIGamePlayer();
            player0.Token = new QueuedMessageProcessor();
            gm.Join(player0);
        }
       

        private static void ConvertBytesToMessage(object sender, SocketAsyncEventArgs eventArgs)
        {
            QueuedMessageProcessor token = (QueuedMessageProcessor) eventArgs.UserToken;
            short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
            byte[] m = eventArgs.Buffer.Skip(eventArgs.Offset).Take(messageLength).ToArray();
            
            IDragonMarbleGameMessage gameMessage = GameMessageFactory.GetGameMessage(m);
            Logger.DebugFormat("receivec. {0}", gameMessage.MessageType);
            token.ReceivedMessage = gameMessage;
            
            if (gameMessage.MessageType == GameMessageType.RollMoveDice)
            {   
                RollMoveDiceResultGameMessage rollMoveDiceResultContent  = new RollMoveDiceResultGameMessage()
                {
                    To = Guid.NewGuid(),
                    From = Guid.NewGuid(),
                    Dices = new List<Int32>(new[] {2,3})
                };
                token.SendingMessage = rollMoveDiceResultContent;

                gm.Notify(Guid.NewGuid(),
                    GameMessageType.RollMoveDice, rollMoveDiceResultContent);
            }

            if (gameMessage.MessageType == GameMessageType.OrderCardSelect)
            {
                OrderCardSelectGameMessage gameMessage2 = (OrderCardSelectGameMessage) gameMessage;
                gameMessage2.From = Guid.NewGuid();
                gameMessage2.To= Guid.NewGuid();
                gameMessage2.Result = true;
                Int16 foo = gameMessage2.SelectedCardNumber;
                gameMessage2.OrderCardSelectState[foo] = (short) ((GamePlayer) token.Player).Order;
                token.SendingMessage = gameMessage;
            }
        }
    }

    internal class AIGamePlayer : GamePlayer
    {
        public AIGamePlayer()
        {
            Info.ControlMode = StageUnitInfo.ControlModeType.AI_0;
        }

    }
}