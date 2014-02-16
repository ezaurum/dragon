using System;
using System.Threading;

namespace Dragon
{
    public class HeartBeatReceiver : IDisposable
    {
        private long _lastBeat;
        private readonly int _threshold;
        

        public long LastBeat
        {
            set { _lastBeat = value; }
        }

        public event Action OnBeatStop;

        public void Beat()
        {
            Interlocked.Increment(ref _lastBeat);
        }

        public HeartBeatReceiver(int threshold = 5)
        {
            _threshold = threshold;
        }

        public void CheckBeat(HeartBeatChecker checker, long obj)
        {
            if (0 == _lastBeat) return;

            if (Interlocked.Read(ref _lastBeat) >= obj - _threshold)
            {
                Interlocked.Exchange(ref _lastBeat, obj);
                return;
            }

            checker.OnBeat -= CheckBeat;
            if (null != OnBeatStop) OnBeatStop();
        }

        public void Dispose()
        {
            OnBeatStop = null;
        }
    }
}