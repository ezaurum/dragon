﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Dragon.Interfaces;
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
                new IPEndPoint(IPAddress.Any, Port), new MessageProcessorProvier<IDragonMarbleGameMessage>());
            server.OnReceiveBytes += ConvertBytesToMessage;
            server.OnAcceptConnection += AddPlayer;
            server.Start();

            Console.ReadKey();
        }

        private static void AddPlayer(object sender, SocketAsyncEventArgs eventArgs)
        {   
            Logger.Debug("connectecd.");
            QueuedMessageProcessor<IDragonMarbleGameMessage> token = (QueuedMessageProcessor<IDragonMarbleGameMessage>)eventArgs.UserToken;
            //Logger.DebugFormat("token guid : {0}",token.Id);
            GamePlayer player = new GamePlayer {
                Id = Guid.NewGuid(),
                MessageProcessor = token, 
                Order = 0,
                TeamColor = StageUnitInfo.UNIT_COLOR.BLUE,
                CharacterId = 1,
                Gold = 2000000
            };
            token.Player = player;
            
            gm.Join(player);

            //TODO dummy ai player
            GamePlayer player0 = new AIGamePlayer
            {
                Id = Guid.NewGuid(),
                Order = 1,
                TeamColor = StageUnitInfo.UNIT_COLOR.GREEN,
                CharacterId = 2,
                Gold = 2000000
            };
            gm.Join(player0);

            if (gm.IsGameStartable)
            {
                gm.StartGame();
            }
        }

        private static void ConvertBytesToMessage(object sender, SocketAsyncEventArgs eventArgs)
        {
            QueuedMessageProcessor<IDragonMarbleGameMessage> token = (QueuedMessageProcessor<IDragonMarbleGameMessage>)eventArgs.UserToken;
            short messageLength = BitConverter.ToInt16(eventArgs.Buffer, eventArgs.Offset);
            byte[] m = eventArgs.Buffer.Skip(eventArgs.Offset).Take(messageLength).ToArray();
            
            IDragonMarbleGameMessage gameMessage = GameMessageFactory.GetGameMessage(m);
            Console.WriteLine("received {0} message from {1}. ", gameMessage.MessageType, gameMessage.From);
            token.ReceivedMessage = gameMessage;
            
            if (gameMessage.MessageType == GameMessageType.RollMoveDice)
            {   
                RollMoveDiceResultGameMessage rollMoveDiceResultContent  = new RollMoveDiceResultGameMessage()
                {
                    To = Guid.NewGuid(),
                    From = Guid.NewGuid(),
                    Dices = new List<char>(new[] {(char)2,(char)3})
                };
                token.SendingMessage = rollMoveDiceResultContent;

                /*gm.Notify(Guid.NewGuid(),
                    GameMessageType.RollMoveDice, rollMoveDiceResultContent);*/
            }

            if (gameMessage.MessageType == GameMessageType.OrderCardSelect)
            {
                OrderCardSelectGameMessage gameMessage2 = (OrderCardSelectGameMessage) gameMessage;
                Int16 foo = gameMessage2.SelectedCardNumber;

                gameMessage2.To = gameMessage2.From;
                gameMessage2.From = Guid.NewGuid();
                gameMessage2.OrderCardSelectState[foo] = true;

                gm.SelectOrder(foo, ((GamePlayer)token.Player));

                Int16 foo1 = (short) (foo == 0 ? 1 : 0);
                OrderCardSelectGameMessage m3 = new OrderCardSelectGameMessage
                {
                    SelectedCardNumber = -1,
                    From = gameMessage2.From,
                    To = gameMessage2.To,
                    OrderCardSelectState = new List<bool> {true, true},
                    NumberOfPlayers = 2
                };
                gm.SelectOrder(foo1, gm.Players[1]);
                

                Int16 f = (short) new Random().Next(0, gameMessage2.NumberOfPlayers);
                //
                OrderCardResultGameMessage orderCardResultGameMessage = new OrderCardResultGameMessage()
                {
                    FirstCardNumber = f,
                    NumberOfPlayers = gameMessage2.NumberOfPlayers,
                    From = gameMessage2.From,
                    To = gameMessage2.To,
                    FirstPlayerId = gm.GetId(f)
                };

                token.SendingMessage = gameMessage2;
                token.SendingMessage = m3;
                token.SendingMessage = orderCardResultGameMessage;

                //TODO 처리가 필요
                IGameMessage receivedMessage = token.ReceivedMessage;

                //TODO async call. event or something?
                Task.Factory.StartNew(gm.OrderEnd);
            }
        }
    }
}