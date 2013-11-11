using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Dragon;
using DragonMarble.Account;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public class SessionManager : ISessionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SessionManager));

        public SessionManager()
        {
            Session = new Dictionary<Guid, GameAccountInfo>();
            AccountSession = new Dictionary<Guid, Guid>();
        }

        public Dictionary<Guid, GameAccountInfo> Session { get; set; }
        public Dictionary<Guid, Guid> AccountSession { get; set; }

        public void Login(object sender, SocketAsyncEventArgs e)
        {
            //check key from session
            Raja token = (Raja) e.UserToken;

            IDragonMarbleGameMessage dragonMarbleGameMessage = token.ReceivedMessage;

            switch (dragonMarbleGameMessage.MessageType)
            {
                case GameMessageType.Session:
                    SessionGameMessage message = (SessionGameMessage) dragonMarbleGameMessage;
                    SetSessionValues(message.SessionKey, message.GameAccountId, token, message);
                    break;

                case GameMessageType.RequestSession:
                    RequestSessionGameMessage rsgm = (RequestSessionGameMessage) dragonMarbleGameMessage;
                
                //logged on already
                    if (AccountSession.ContainsKey(rsgm.GameAccountId))
                    {
                        //TODO
                        Guid sessionId = AccountSession[rsgm.GameAccountId];
                        SetSessionValues(sessionId, rsgm.GameAccountId, token, new SessionGameMessage
                        {
                            GameAccountId = rsgm.GameAccountId,
                            SessionKey = sessionId
                        });
                    }
                    else
                    {
                        //TODO
                        SetSessionValues(GetNewSessionId(), rsgm.GameAccountId, token, new SessionGameMessage
                        {
                            GameAccountId = rsgm.GameAccountId
                        });
                    }
                    break;
                case GameMessageType.ValidateSession:
                    break;
            }
        }

        private void SetSessionValues(Guid sessionKey, Guid gameAccountId, Raja token, SessionGameMessage message)
        {
            GameAccountInfo gameAccountInfo;
            if (Session.ContainsKey(sessionKey))
            {
                Logger.DebugFormat("Session key {0} is exist.", sessionKey);
                gameAccountInfo = Session[sessionKey];
            }
            else
            {
                Logger.DebugFormat("Session key {0} is not exist.", sessionKey);
                gameAccountInfo = GetGameAccount(gameAccountId);
                sessionKey = GetNewSessionId();
                Session.Add(sessionKey, gameAccountInfo);
                token.SendingMessage = message;
            }

            token.GameAccount = gameAccountInfo;
        }

        private Guid GetNewSessionId()
        {
            //TODO is this unique?
            //세션키 만드는 범위를 각각 지정해 줘야 하나? 
            //game instance 마다 지정이 안 된다면 일일이 체크를 해야 할 텐데.
            return Guid.NewGuid();
        }

        private GameAccountInfo GetGameAccount(Guid gameAccountId)
        {
            return new GameAccountInfo
            {
                Id = gameAccountId
            };
        }
    }
}