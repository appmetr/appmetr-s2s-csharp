namespace AppmetrS2S.Persister
{
    using System.Collections.Generic;
    using Actions;
    using System;

    public interface IBatchPersister
    {
        /// <summary>
        /// Get the oldest batch from storage, but dont remove it.
        /// </summary>
        Batch GetNext();

        /// <summary>
        /// Persist list of events as Batch.
        /// </summary>
        /// <param name="actionList">actionList list of events.</param>
        void Persist(List<AppMetrAction> actionList);

        /// <summary>
        /// Remove oldest batch from storage.
        /// </summary>
        void Remove();

        /// <summary>
        /// Setup server Id for batch identification
        /// </summary>
        /// <param name="serverId">server id to identify batches</param>
        void SetServerId(String serverId);
    }
}
