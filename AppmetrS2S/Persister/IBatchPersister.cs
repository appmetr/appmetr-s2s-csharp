namespace AppmetrS2S.Persister
{
    using System.Collections.Generic;
    using Actions;

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
    }
}