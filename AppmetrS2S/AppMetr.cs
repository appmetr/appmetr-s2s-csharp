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
        private static readonly ILog Log = LogManager.GetLogger<AppMetr>();

        private readonly string _token;
        private readonly string _url;
        private readonly IBatchPersister _batchPersister;
        private readonly HttpRequestService _httpRequestService;

        private bool _stopped;
        private readonly List<AppMetrAction> _actionList = new List<AppMetrAction>();

        private readonly object _flushLock = new object();
        private readonly object _uploadLock = new object();

        private readonly AppMetrTimer _flushTimer;
        private readonly AppMetrTimer _uploadTimer;

        private int _eventSize;
        private const int MaxEventsSize = 1024*500*20;

        private const int MillisPerMinute = 1000*60;
        private const int FlushPeriod = MillisPerMinute/2;
        private const int UploadPeriod = MillisPerMinute/2;

        public AppMetr(
            string token,
            string url,
            IBatchPersister batchPersister = null,
            IJsonSerializer serializer = null)
        {
            Log.InfoFormat("Start Appmetr for token={0}, url={1}", token, url);

            _token = token;
            _url = url;
            var jsonSerializer = serializer ?? new BasicJsonSerializer();
            _batchPersister = batchPersister ?? new MemoryBatchPersister(jsonSerializer);
            _httpRequestService = new HttpRequestService();

            _batchPersister.SetServerId(Guid.NewGuid().ToString());

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
                var currentEventSize = action.CalcApproximateSize();

                bool flushNeeded;
                lock (_actionList)
                {
                    _eventSize += currentEventSize;
                    _actionList.Add(action);

                    flushNeeded = _eventSize >= MaxEventsSize;
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

        private void Flush()
        {
            lock (_flushLock)
            {
                List<AppMetrAction> copyActions;
                lock (_actionList)
                {
                    Log.DebugFormat("Flush started for {0} actions", _actionList.Count);

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
                Log.Debug("Upload started");

                byte[] batch;
                var uploadedBatchCounter = 0;
                var allBatchCounter = 0;
                while ((batch = _batchPersister.GetNext()) != null)
                {
                    allBatchCounter++;

                    Log.DebugFormat("Starting send batch");
                    if (_httpRequestService.SendRequest(_url, _token, batch))
                    {
                        Log.DebugFormat("Successfully sent batch");

                        _batchPersister.Remove();
                        uploadedBatchCounter++;

                        Log.DebugFormat("Batch successfully uploaded");
                    }
                    else
                    {
                        Log.ErrorFormat("Error while upload batch");
                        break;
                    }
                }

                Log.DebugFormat("{0} from {1} batches uploaded", uploadedBatchCounter, allBatchCounter);
            }
        }
    }
}
