using System;
using System.Collections.Generic;
using System.Net.Sockets;
using DragonMarble.Account;
using DragonMarble.Message;
using log4net;

namespace DragonMarble
{
    public class SessionManager :ISessionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SessionManager));
        public Dictionary<Guid, GameAccountInfo> Session { get; set; }

        public SessionManager ()
        {
            Session = new Dictionary<Guid, GameAccountInfo>();
        }

        public void Login(object sender, SocketAsyncEventArgs e)
        {
            //check key from session
            Raja token = (Raja)e.UserToken;

            SessionGameMessage message= (SessionGameMessage) token.ReceivedMessage;
            Guid gameAccountId = message.GameAccountId;
            Guid sessionKey = message.SessionKey;

            GameAccountInfo gameAccountInfo;

            if (Session.ContainsKey(sessionKey))
            {
                Logger.DebugFormat("Session key {0} is exist.", sessionKey);
                gameAccountInfo = Session[sessionKey];
            }
            else
            {
                Logger.DebugFormat("Session key {0} is not exist.",sessionKey);
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
                Id=gameAccountId
            };
        }
    }

    public interface ISessionManager
    {
        void Login(object sender, SocketAsyncEventArgs e);
    }
}