namespace AppmetrS2S.Persister
{
    #region using directives

    using System.Collections.Generic;
    using Actions;

    #endregion

    public class MemoryBatchPersister : IBatchPersister
    {
        private readonly Queue<Batch> _batchQueue = new Queue<Batch>();
        private int _batchId = 0;

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
                _batchQueue.Enqueue(new Batch(_batchId++, actionList));
            }
        }

        public void Remove()
        {
            lock (_batchQueue)
            {
                _batchQueue.Dequeue();
            }
        }
    }
}