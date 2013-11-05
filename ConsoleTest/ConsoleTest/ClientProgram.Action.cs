using System;
using Dragon.Client;
using DragonMarble;
using DragonMarble.Message;

namespace ConsoleTest
{
    public partial class ClientProgram
    {
        private static bool CheckAction(string readLine, Unity3DNetworkManager nm)
        {
            if (readLine.Contains("Q") || readLine.Contains("q"))
            {
                return true;
            }

            if (readLine.Contains("RR") || readLine.Contains("rr"))
            {
                nm.Reconnect();
            }

            if (readLine.Contains("ss") || readLine.Contains("SS"))
            {
                nm.SendMessage(new SessionGameMessage()
                {
                    SessionKey = Guid.NewGuid()
                });
                return false;
            }
            
            if (readLine.Contains("c") || readLine.Contains("C"))
            {
                nm.SendMessage((new MakeNewGameRoomGameMessage
                {
                    BoardType = GameBoard.BoardType.DragonNest,
                    PlayType = GamePlayType.Individual2PlayerPlay
                }));
                return false;
            }


            //game
            if (readLine.Contains("D") || readLine.Contains("d"))
            {
                nm.SendMessage(new RollMoveDiceGameMessage
                {
                    Pressed = new Random().Next(0, int.MaxValue)
                });
            }

            if (readLine.Contains("A") || readLine.Contains("a"))
            {
                nm.SendMessage(new ActionResultCopyGameMessage());
            }

            //game waiting room
            if (readLine.Contains("s") || readLine.Contains("S"))
            {
                nm.SendMessage(new StartGameGameMessage());
            }

            if (readLine.Contains("r") || readLine.Contains("R"))
            {
                Console.WriteLine(_unitInfo.IsReady);
                nm.SendMessage(new ReadyStateGameMessage
                {
                    Actor = _unitInfo.Id,
                    Ready = !_unitInfo.IsReady
                });
            }

            if (readLine.Contains("x") || readLine.Contains("X"))
            {
                nm.SendMessage(new ExitWaitingRoomGameMessage
                {
                    Actor = _unitInfo.Id
                });
            }


            //ordering
            if (readLine.Contains("1"))
            {
                OrderCardSelectState[1] = true;
                OrderCardSelectGameMessage orderCardSelectGameMessage = new OrderCardSelectGameMessage
                {
                    SelectedCardNumber = 1,
                    OrderCardSelectState = OrderCardSelectState,
                    NumberOfPlayers = 2
                };
                nm.SendMessage(orderCardSelectGameMessage);
            }

            if (readLine.Contains("0"))
            {
                OrderCardSelectState[0] = true;
                OrderCardSelectGameMessage orderCardSelectGameMessage = new OrderCardSelectGameMessage
                {
                    SelectedCardNumber = 0,
                    OrderCardSelectState = OrderCardSelectState,
                    NumberOfPlayers = 2
                };
                nm.SendMessage(orderCardSelectGameMessage);
            }


            //test
            if (readLine.Contains("I"))
            {
                while (true)
                {
                    nm.SendMessage(new RollMoveDiceGameMessage
                    {
                        Pressed = new Random().Next(0, int.MaxValue)
                    });
                }
            }
            return false;
        }
    }
}