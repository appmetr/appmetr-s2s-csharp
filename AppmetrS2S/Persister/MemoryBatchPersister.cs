using AppmetrS2S.Serializations;

namespace AppmetrS2S.Persister
{
    #region using directives

    using System.Collections.Generic;
    using Actions;
    using System;

    #endregion

    public class MemoryBatchPersister : IBatchPersister
    {
        private readonly Queue<Batch> _batchQueue = new Queue<Batch>();
        private readonly IJsonSerializer _serializer;
        private Int64 _batchId = 0;
        private String _serverId;

        public MemoryBatchPersister(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public byte[] GetNext()
        {
            lock (_batchQueue)
            {
                if (_batchQueue.Count == 0)
                    return null;

                var batch = _batchQueue.Peek();
                var binaryBatch = Utils.SerializeBatch(batch, _serializer);
                return binaryBatch;
            }
        }

        public void Persist(List<AppMetrAction> actionList)
        {
            lock (_batchQueue)
            {
                _batchQueue.Enqueue(new Batch(_serverId, _batchId++, actionList));
            }
        }

        public void Remove()
        {
            lock (_batchQueue)
            {
                _batchQueue.Dequeue();
            }
        }

        public void SetServerId(string serverId)
        {
            _serverId = serverId;
        }
    }
}
