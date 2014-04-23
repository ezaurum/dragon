using System;
using System.Threading;

namespace Dragon
{
    public class HeartBeatReceiver<TAck> : IDisposable
    {
        public TimeSpan Threshold { get; set; }
        public byte FailedLimit { get; set; }
        private DateTime _lastTime;
        private int _failed;

        public DateTime LastTime
        {
            set { _lastTime = value; }
        }

        public event Action OnBeatStop;

        public HeartBeatReceiver()
        {
            Threshold = TimeSpan.FromSeconds(2);
            FailedLimit = 1;
        }

        public HeartBeatReceiver(TimeSpan threshold, byte failedLimit)
        {
            Threshold = threshold;
            FailedLimit = failedLimit;
        }

        public void CheckBeat(HeartBeatChecker checker, DateTime time)
        {
            if (time - _lastTime < Threshold)
            {
                Interlocked.Exchange(ref _failed, 0);
                return;
            }

            if (Interlocked.Increment(ref _failed) < FailedLimit) return;

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
            if (null != ReceiveHeartbeat) ReceiveHeartbeat(beat, errorCode);
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