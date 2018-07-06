﻿namespace AppmetrS2S.Persister
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Actions;

    #endregion

    [DataContract]
    [KnownType(typeof(AttachProperties))]
    [KnownType(typeof(Event))]
    [KnownType(typeof(Level))]
    [KnownType(typeof(Payment))]
    [KnownType(typeof(TrackSession))]
    public class Batch
    {
        [DataMember(Name = "batchId")]
        private readonly Int64 _batchId;

        [DataMember(Name = "batch")]
        private readonly List<AppMetrAction> _batch;

        [DataMember(Name = "serverId")]
        private readonly String _serverId;

        private Batch()
        {
            
        }

        public Batch(String serverId, Int64 batchId, IEnumerable<AppMetrAction> actionList)
        {
            _serverId = serverId;
            _batchId = batchId;
            _batch = new List<AppMetrAction>(actionList);
        }

        public Int64 GetBatchId()
        {
            return _batchId;
        }

        public List<AppMetrAction> GetBatch()
        {
            return _batch;
        }

        public override String ToString()
        {
            return String.Format("Batch{{events={0}, batchId={1}, serverId={2}}", _batch.Count, _batchId, _serverId);
        }
    }
}
