using System;
using System.Threading;

namespace Dragon
{
    public class HeartBeatReceiver : IDisposable
    {
        private long _lastBeat;
        private readonly int _threshold;

        private int _failed;

        public long LastBeat
        {
            set { _lastBeat = value; }
        }

        public event Action OnBeatStop;

        public void Beat()
        {
            Interlocked.Increment(ref _lastBeat);
        }

        public HeartBeatReceiver(int threshold = 2)
        {
            _threshold = threshold;
        }

        public void CheckBeat(HeartBeatChecker checker, long obj)
        {
            if (Interlocked.Read(ref _lastBeat) < obj)
            {
                Interlocked.Increment(ref _failed);
            }            

            if (_failed < _threshold) return;

            Interlocked.Exchange(ref _lastBeat, obj);

            checker.OnBeat -= CheckBeat;
            if (null != OnBeatStop) OnBeatStop();
        }

        public void Dispose()
        {
            OnBeatStop = null;
        }

        public override string ToString()
        {
            return string.Format("LastBeat: {0}, Threshold: {1}, Failed: {2}", _lastBeat, _threshold, _failed);
        }
    }
}