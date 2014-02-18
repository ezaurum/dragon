using System;

namespace Dragon
{
    public class HeartBeatReceiver : IDisposable
    {
        private readonly TimeSpan _threshold;

        private DateTime _lastTime;

        public DateTime LastTime
        {
            set { _lastTime = value; }
        }

        public event Action OnBeatStop;

        public void Beat()
        {
            _lastTime = DateTime.Now;
        }

        public HeartBeatReceiver(int threshold = 2)
        {
            _threshold = TimeSpan.FromSeconds(threshold);
        }

        public void CheckBeat(HeartBeatChecker checker, DateTime time)
        {
            if (_lastTime - time < _threshold) return;
            
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