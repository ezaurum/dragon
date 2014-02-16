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

        public HeartBeatChecker(int interval= 1000)
        {
            var timer = new Timer { Interval = interval };
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