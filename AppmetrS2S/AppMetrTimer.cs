namespace AppmetrS2S
{
    #region using directives

    using System;
    using System.Threading;
    using log4net;

    #endregion

    public class AppMetrTimer
    {
        private static readonly ILog _log = LogUtils.GetLogger(typeof (AppMetrTimer));

        private readonly int _period;
        private readonly Action _onTimer;
        private readonly string _jobName;

        private readonly object _lock = new object();
        private bool _run;

        public AppMetrTimer(int period, Action onTimer, string jobName = "AppMetrTimer")
        {
            _period = period;
            _onTimer = onTimer;
            _jobName = jobName;
        }

        public void Start()
        {
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat("Start {0} with period {1}", _jobName, _period);
            }

            _run = true;
            while (_run)
            {
                lock (_lock)
                {
                    try
                    {
                        Monitor.Wait(_lock, _period);

                        _log.InfoFormat("{0} triggered", _jobName);
                        _onTimer.Invoke();
                    }
                    catch (ThreadInterruptedException)
                    {
                        _log.WarnFormat("{0} interrupted", _jobName);
                        _run = false;
                    }
                    catch (Exception e)
                    {
                        _log.ErrorFormat("{0} unhandled exception:\r\n{1}", _jobName, e);
                    }
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
            _log.InfoFormat("{0} stop triggered", _jobName);
            _run = false;
            Thread.CurrentThread.Interrupt();
        }
    }
}