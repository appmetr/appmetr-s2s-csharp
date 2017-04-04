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
        private int _batchId = 0;
        private String _serverId;

        public Batch GetNext()
        {
            lock (_batchQueue)
            {
                return _batchQueue.Count == 0 ? null : _batchQueue.Peek();
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
