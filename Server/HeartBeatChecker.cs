using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Dragon
{
    public class HeartBeatChecker
    { 
        private DateTime _time;

        public HeartBeatChecker(int interval= 750)
        {
            var timer = new Timer { Interval = interval };
            timer.Elapsed += CheckBeat;
            OnBeat += AddLastBeat;
            timer.Start();
        }

        public event Action<HeartBeatChecker, DateTime> OnBeat;

        private void AddLastBeat(HeartBeatChecker checker, DateTime dateTime)
        {
            _time = DateTime.Now;
        }

        private void CheckBeat(object sender, ElapsedEventArgs e)
        {
            OnBeat(this, _time);
        }

        public override string ToString()
        {
            return string.Format("Time: {0}", _time);
        }
    }
}