using AppmetrS2S.Serializations;
using Common.Logging;

namespace AppmetrS2S
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Actions;
    using Persister;

    #endregion

    public class AppMetr
    {
        private static readonly ILog _log = LogManager.GetLogger<AppMetr>();

        private readonly string _token;
        private readonly string _url;
        private readonly IBatchPersister _batchPersister;
        private readonly HttpRequestService _httpRequestService;

        private bool _stopped;
        private readonly List<AppMetrAction> _actionList = new List<AppMetrAction>();

        private readonly object _flushLock = new object();
        private readonly object _uploadLock = new object();

        private readonly AppMetrTimer _uploadTimer;

        private int _eventSize;
        private DateTime _lastFlushTime;

        private const int MaxEventsSize = 2 * 1024 * 1024; //2 MB
        private const int MillisPerMinute = 1000*60;
        private const int FlushPeriod = MillisPerMinute/2;
        private const int UploadPeriod = MillisPerMinute/2;

        public AppMetr(
            string token,
            string url,
            IBatchPersister batchPersister = null,
            IJsonSerializer serializer = null)
        {
            _log.InfoFormat("Start Appmetr for token={0}, url={1}", token, url);

            _token = token;
            _url = url;
            _batchPersister = batchPersister ?? new MemoryBatchPersister();
            _httpRequestService = new HttpRequestService(serializer ?? new BasicJsonSerializer());

            _batchPersister.SetServerId(Guid.NewGuid().ToString());

            _uploadTimer = new AppMetrTimer(UploadPeriod, Upload, "UploadJob");
            new Thread(_uploadTimer.Start).Start();

            _lastFlushTime = DateTime.UtcNow;
        }

        public void Track(AppMetrAction action)
        {
            if (_stopped)
            {
                throw new Exception("Trying to track after stop!");
            }

            try
            {
                var currentEventSize = action.CalcApproximateSize();

                lock (_actionList)
                {
                    _eventSize += currentEventSize;
                    _actionList.Add(action);

                    if (FlushNeeded())
                    {
                        Flush();
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Track failed", e);
            }
        }

        public Boolean FlushIfNeeded()
        {
            lock (_actionList)
            {
                if (FlushNeeded())
                {
                    Flush();
                    return true;
                }
            }

            return false;
        }

        protected Boolean FlushNeeded()
        {
             return _eventSize >= MaxEventsSize || (DateTime.UtcNow - _lastFlushTime).TotalMilliseconds > FlushPeriod;
        }


        public void Stop()
        {
            _log.Info("Stop appmetr");

            _stopped = true;

            lock (_uploadLock)
            {
                _uploadTimer.Stop();
            }

            Flush();
        }

        private void Flush()
        {
            lock (_flushLock)
            {
                List<AppMetrAction> copyActions;
                lock (_actionList)
                {
                    _log.DebugFormat("Flush started for {0} actions", _actionList.Count);

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
                    _log.Info("Nothing to flush");
                }

                _lastFlushTime = DateTime.UtcNow;
            }
        }

        private void Upload()
        {
            lock (_uploadLock)
            {
                _log.Debug("Upload started");

                Batch batch;
                var uploadedBatchCounter = 0;
                var allBatchCounter = 0;
                while ((batch = _batchPersister.GetNext()) != null)
                {
                    allBatchCounter++;

                    _log.DebugFormat("Starting send batch with id={0}", batch.GetBatchId());
                    if (_httpRequestService.SendRequest(_url, _token, batch))
                    {
                        _log.DebugFormat("Successfuly send batch with id={0}", batch.GetBatchId());

                        _batchPersister.Remove();
                        uploadedBatchCounter++;

                        _log.DebugFormat("Batch {0} successfully uploaded", batch.GetBatchId());
                    }
                    else
                    {
                        _log.ErrorFormat("Error while upload batch {0}", batch.GetBatchId());
                        break;
                    }
                }

                _log.DebugFormat("{0} from {1} batches uploaded", uploadedBatchCounter, allBatchCounter);
            }
        }
    }
}