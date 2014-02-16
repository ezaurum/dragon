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
            if (Interlocked.Read(ref _lastBeat) > obj)
            {
                Interlocked.Exchange(ref _lastBeat, obj);
                return;
            }
            
            if (Interlocked.Read(ref _lastBeat) >= obj - _threshold) return;

            checker.OnBeat -= CheckBeat;
            if (null != OnBeatStop) OnBeatStop();
        }

        public void Dispose()
        {
            OnBeatStop = null;
        }
    }
}