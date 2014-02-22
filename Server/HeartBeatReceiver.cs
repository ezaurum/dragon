using System;

namespace Dragon
{
    public class HeartBeatReceiver : IDisposable
    {
        private static readonly TimeSpan Threshold = TimeSpan.FromSeconds(2);
        private const byte FailedLimit = 5;

        private DateTime _lastTime;
        private byte _failed;

        public DateTime LastTime
        {
            set { _lastTime = value; }
        }

        public event Action OnBeatStop;

        public void Beat()
        {
            _lastTime = DateTime.Now;
        }

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