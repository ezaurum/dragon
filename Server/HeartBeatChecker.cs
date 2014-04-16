using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Dragon
{
    public class HeartBeatChecker
    { 
        private DateTime _time;
        private Timer _timer;

        public HeartBeatChecker(int interval= 120000)
        {
            _timer = new Timer { Interval = interval };
            _timer.Elapsed += CheckBeat;
            OnBeat += AddLastBeat;
            _timer.Start();
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

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}