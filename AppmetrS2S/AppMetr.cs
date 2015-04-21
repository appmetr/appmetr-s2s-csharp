namespace AppmetrS2S
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Actions;
    using log4net;
    using Persister;

    #endregion

    public class AppMetr
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (AppMetr));

        private String _token;
        private String _url;
        private IBatchPersister _batchPersister;

        private bool _stopped = false;
        private readonly List<AppMetrAction> _actionList = new List<AppMetrAction>();

        private readonly object _flushLock = new object();
        private readonly object _uploadLock = new object();

        private readonly AppMetrTimer _flushTimer;
        private readonly AppMetrTimer _uploadTimer;

        private int _eventSize = 0;
        private const int MaxEventsSize = 1024*500*20;//2 MB

        private const int MillisPerMinute = 1000*60;
        private const int FlushPeriod = MillisPerMinute/2;
        private const int UploadPeriod = MillisPerMinute/2;

        public AppMetr(String token, String url, IBatchPersister batchPersister = null)
        {
            Log.InfoFormat("Start Appmetr for token={0}, url={1}", token, url);

            _token = token;
            _url = url;
            _batchPersister = batchPersister ?? new MemoryBatchPersister();

            _flushTimer = new AppMetrTimer(FlushPeriod, Flush, "FlushJob");
            new Thread(_flushTimer.Start).Start();

            _uploadTimer = new AppMetrTimer(UploadPeriod, Upload, "UploadJob");
            new Thread(_uploadTimer.Start).Start();
        }

        public void Track(AppMetrAction action)
        {
            if (_stopped)
            {
                throw new Exception("Trying to track after stop!");
            }

            try
            {
                bool flushNeeded;
                lock (_actionList)
                {
                    Interlocked.Add(ref _eventSize, action.CalcApproximateSize());
                    _actionList.Add(action);

                    flushNeeded = IsNeedToFlush();
                }

                if (flushNeeded)
                {
                    _flushTimer.Trigger();
                }
            }
            catch (Exception e)
            {
                Log.Error("Track failed", e);
            }
        }

        public void Stop()
        {
            Log.Info("Stop appmetr");

            _stopped = true;

            lock (_uploadLock)
            {
                _uploadTimer.Stop();
            }

            lock (_flushLock)
            {
                _flushTimer.Stop();
            }

            Flush();
        }

        private bool IsNeedToFlush()
        {
            return _eventSize >= MaxEventsSize;
        }

        private void Flush()
        {
            lock (_flushLock)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Flush started for {0} actions", _actionList.Count);
                }

                List<AppMetrAction> copyActions;
                lock (_actionList)
                {
                    copyActions = new List<AppMetrAction>(_actionList);
                    _actionList.Clear();
                    _eventSize = 0;
                }

                if (copyActions.Count > 0)
                {
                    _batchPersister.Persist(copyActions);
                    _uploadTimer.Trigger();
                }
                else
                {
                    Log.Info("Nothing to flush");
                }
            }
        }

        private void Upload()
        {
            lock (_uploadLock)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Upload started");
                }

                Batch batch;
                int uploadedBatchCounter = 0;
                int allBatchCounter = 0;
                while ((batch = _batchPersister.GetNext()) != null)
                {
                    allBatchCounter++;

                    Log.DebugFormat("Starting send batch with id={0}", batch.GetBatchId());
                    if (HttpRequestService.SendRequest(_url, _token, batch))
                    {
                        Log.DebugFormat("Successfuly send batch with id={0}", batch.GetBatchId());

                        _batchPersister.Remove();
                        uploadedBatchCounter++;

                        if (Log.IsDebugEnabled)
                        {
                            Log.DebugFormat("Batch {0} successfully uploaded", batch.GetBatchId());
                        }
                    }
                    else
                    {
                        Log.ErrorFormat("Error while upload batch {0}", batch.GetBatchId());
                        break;
                    }
                }

                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("{0} from {1} batches uploaded", uploadedBatchCounter, allBatchCounter);
                }
            }
        }
    }
}