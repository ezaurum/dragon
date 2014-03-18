using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Dragon
{
    public class BlockSender<TK,TReq,TAck,TSender>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BlockSender<TK,TReq,TAck,TSender>));

        #region common waiting

        private readonly ConcurrentDictionary<TK, ManualResetEventSlim> _waitings =
            new ConcurrentDictionary<TK, ManualResetEventSlim>();

        private readonly ConcurrentDictionary<TK, TSender> _waitingSessions =
            new ConcurrentDictionary<TK, TSender>();

        private readonly ConcurrentDictionary<TK, int> _failedCodes =
            new ConcurrentDictionary<TK, int>();

        private readonly object _lock = new object();
        protected readonly object Lock2 = new object();

        public ClientDragonSocket<TReq, TAck> Socket { private get; set; }

        public bool Send(TReq req, TSender session, out TAck ack)
        {
            Logger.Info(req);
            Socket.Send(req);

            bool result = false;
            //            Action action = () => result = WaitingForResponse(session, out ack);
            //          TaskWaiter(action);

            ack = default(TAck);
            return result;
        }

        private static void TaskWaiter(Action action)
        {
            var task = new Task(action);
            task.Start();
            task.Wait();
        }

        private bool WaitingForResponse(TSender session, out TAck ack)
        {
            var mres = new ManualResetEventSlim();
            ack = default(TAck);
            lock (_lock)
            {
        /*        if (!_waitings.TryAdd(session, mres))
                {
                    return false;
                }

                if (!_waitingSessions.TryAdd(session, session))
                {
                    _waitings.TryRemove(session, out mres);

                    return false;
                }*/
            }

            mres.Reset();

            //semapore stop
            //TODO
            mres.Wait(321);

            int code = 0;
/*            if (!_waitings.ContainsKey(session.Id) && !_failedCodes.TryRemove(session.Id, out code)) return true;*/
            
/*            Logger.DebugFormat("session id : {0}, error :{1}", session.Id, code);*/
            
            return false;
        }

        #endregion

        #region after item instace made

        public void Success(TK miSessionId)
        {
            ManualResetEventSlim mres;
            TSender session;
            if (IsHandleInvalid(miSessionId, out session, out mres)) return;


            mres.Set();
        }



        #endregion

        public void Failed(TK miSessionId, int errorCode)
        {
            ManualResetEventSlim mres;
            TSender session;
            if (IsHandleInvalid(miSessionId, out session, out mres)) return;

            _failedCodes.TryAdd(miSessionId, errorCode);

            mres.Set();
        }

        private bool IsHandleInvalid(TK miSessionId, out TSender session, out ManualResetEventSlim mres)
        {
            mres = null;
            session = default(TSender);
            if (!_waitings.ContainsKey(miSessionId))
            {
                Logger.FatalFormat("not requeste reset. {0}", miSessionId);
                return true;
            }

            if (!_waitingSessions.TryRemove(miSessionId, out session))
            {
                Logger.FatalFormat("not requeste. {0}", miSessionId);
                return true;
            }

            _waitings.TryRemove(miSessionId, out mres);
            return false;
        }

        
    }
}