using System;
using System.Threading;

namespace Dragon
{
    public class HeartBeatReceiver<TAck> : IDisposable
    { 
// ReSharper disable once StaticFieldInGenericType
        private static readonly TimeSpan Threshold = TimeSpan.FromSeconds(0.375);
        private const byte FailedLimit = 3;

        private DateTime _lastTime;
        private int _failed;

        public DateTime LastTime
        {
            set { _lastTime = value; }
        }

        public event Action OnBeatStop;

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
            if(null != ReceiveHeartbeat) ReceiveHeartbeat(beat, errorCode);
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