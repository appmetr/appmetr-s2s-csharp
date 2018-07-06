using System;
using System.IO;
using System.Text;
using AppmetrS2S.Persister;
using AppmetrS2S.Serializations;
using log4net;

namespace AppmetrS2S
{
    internal class Utils
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof(FileBatchPersister));

        public static long GetNowUnixTimestamp()
        {
            return (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static byte[] SerializeBatch(Batch batch, IJsonSerializer serializer)
        {
            var json = serializer.Serialize(batch);
            byte[] data = Encoding.UTF8.GetBytes(json);
            return data;
        }

        public static void WriteData(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void WriteBatch(Stream stream, Batch batch, IJsonSerializer serializer)
        {
            Log.DebugFormat("Starting serialize batch with id={0}", batch.GetBatchId());
            var json = serializer.Serialize(batch);
            Log.DebugFormat("Get bytes from serialized batch with id={0}", batch.GetBatchId());
            byte[] data = Encoding.UTF8.GetBytes(json);
            Log.DebugFormat("Write bytes to stream. Batch id={0}", batch.GetBatchId());
            stream.Write(data, 0, data.Length);
            Log.DebugFormat("Flush stream. Batch id={0}", batch.GetBatchId());
            stream.Flush();
        }

        public static bool TryReadBatch(Stream stream, IJsonSerializer serializer, out Batch batch)
        {
            try
            {
                batch = serializer.Deserialize<Batch>(new StreamReader(stream).ReadToEnd());
                Log.InfoFormat("Successfully read the batch {0}", batch.GetBatchId());
                return true;
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("Error while deserialization batch", e);    
                }
                
                batch = null;
                return false;
            }
        }
    }
}