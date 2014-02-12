using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Dragon
{
    public class HeartBeatChecker
    {
        private long _beat;

        public long Beat { get { return _beat; } }

        public HeartBeatChecker()
        {
            var timer = new Timer { Interval = 1000 };
            timer.Elapsed += CheckBeat;
            OnBeat += AddLastBeat;
            timer.Start();
        }

        public event Action<HeartBeatChecker, long> OnBeat;

        private void AddLastBeat(HeartBeatChecker checker, long l)
        {
            Interlocked.Increment(ref _beat);
        }

        private void CheckBeat(object sender, ElapsedEventArgs e)
        {
            OnBeat(this, _beat);
        }
    }
}