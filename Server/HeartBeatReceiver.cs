using System;

namespace Dragon
{
    public class HeartBeatReceiver<TAck> : IDisposable
    { 
// ReSharper disable once StaticFieldInGenericType
        private static readonly TimeSpan Threshold = TimeSpan.FromSeconds(0.5);
        private const byte FailedLimit = 2; 

        private DateTime _lastTime;
        private byte _failed;

        public DateTime LastTime
        {
            set { _lastTime = value; }
        }

        public event Action OnBeatStop;

        public void CheckBeat(HeartBeatChecker checker, DateTime time)
        {
            if (time - _lastTime < Threshold)
            {
                if (_failed > 0) _failed--;
                return;
            }
            
            _failed++;
            if (_failed < FailedLimit) return;

            checker.OnBeat -= CheckBeat;
            if (null != OnBeatStop) OnBeatStop();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedField.Global
        public Func<TAck, bool> IsHeartBeat;

        public event Action<TAck, int> ReceiveHeartbeat;

        public void Receive(TAck beat, int errorCode)
        {
            _lastTime = DateTime.Now;
            if(null != ReceiveHeartbeat) ReceiveHeartbeat(beat, errorCode);
        }

        public void Dispose()
        {
            OnBeatStop = null;
        }

        public override string ToString()
        {
            return string.Format("LastTime: {0}", _lastTime);
        }
    } 
}