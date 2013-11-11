using System;
using DragonMarble.Account;

namespace DragonMarble.Session
{
    public class DragonMarbleGameSession : Dragon.Session.GameSession
    {
        public static readonly TimeSpan SessionExpire = new TimeSpan(0, 30, 0);

        private DateTime _lastLogin;
        public Guid GameRoomId { get; set; }

        public bool IsPlaying
        {
            get
            {
                return Guid.Empty != GameRoomId;
            }
        }

        public DateTime LastLogin
        {
            get { return _lastLogin; }
            set
            {
                _lastLogin = value;
                ExpireTime = _lastLogin;
                ExpireTime = value + SessionExpire;
            } 
        }

        public DateTime ExpireTime { get; private set; }

        public bool IsLoggedIn
        {
            get
            {
                return !(IsExpired || Guid.Empty == Id);
            } 
        }

        public bool IsExpired
        {
            get { return ExpireTime > DateTime.Now; }
        }

        public GameAccountInfo GameAccount { get; set; }
        
        public override void Dispose()
        {
            GameAccount = null;
            GameRoomId = Guid.Empty;
            base.Dispose();
        }
    }
}