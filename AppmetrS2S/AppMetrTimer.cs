namespace AppmetrS2S
{
    #region using directives

    using System;
    using System.Threading;
    using log4net;

    #endregion

    public class AppMetrTimer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (AppMetrTimer));

        private readonly int _period;
        private readonly Action _onTimer;
        private readonly String _jobName;

        private readonly object _lock = new object();
        private bool _run;

        public AppMetrTimer(int period, Action onTimer, String jobName = "AppMetrTimer")
        {
            _period = period;
            _onTimer = onTimer;
            _jobName = jobName;
        }

        public void Start()
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Start {0} with period {1}", _jobName, _period);
            }

            _run = true;
            while (_run)
            {
                bool isTaken = false;
                Monitor.Enter(_lock, ref isTaken);
                try
                {
                    Monitor.Wait(_lock, _period);
                    
                    if (Log.IsInfoEnabled)
                    {
                        Log.InfoFormat("{0} triggered", _jobName);
                    }
                    _onTimer.Invoke();
                }
                catch (ThreadInterruptedException e)
                {
                    Log.WarnFormat("{0} interrupted", _jobName);
                    _run = false;
                }
                finally
                {
                    if (isTaken) Monitor.Exit(_lock);
                }
            }
        }

        public void Trigger()
        {
            bool isTaken = false;
            Monitor.Enter(_lock, ref isTaken);
            try
            {
                Monitor.Pulse(_lock);
            }
            finally
            {
                if (isTaken) Monitor.Exit(_lock);
            }
            
        }

        public void Stop()
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("{0} stop triggered", _jobName);
            }

            _run = false;
            Thread.CurrentThread.Interrupt();
        }
    }
}