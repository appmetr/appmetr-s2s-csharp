using Newtonsoft.Json;

namespace AppmetrS2S.Persister
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Actions;

    #endregion

    public class Batch
    {
        [JsonProperty("batchId")]
        private readonly long _batchId;

        [JsonProperty("batch")]
        private readonly List<AppMetrAction> _batch;

        [JsonProperty("serverId")]
        private readonly string _serverId;

        private Batch()
        {
            
        }

        public Batch(string serverId, long batchId, IEnumerable<AppMetrAction> batch)
        {
            _serverId = serverId;
            _batchId = batchId;
            _batch = new List<AppMetrAction>(batch);
        }

        public long GetBatchId()
        {
            return _batchId;
        }

        public List<AppMetrAction> GetBatch()
        {
            return _batch;
        }

        public override string ToString()
        {
            return string.Format("Batch{{events={0}, batchId={1}, serverId={2}}", _batch.Count, _batchId, _serverId);
        }
    }
}
