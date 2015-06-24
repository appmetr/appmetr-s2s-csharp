using System.IO;

namespace AppmetrS2S
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Web;
    using log4net;
    using Persister;

    #endregion

    internal class HttpRequestService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (HttpRequestService));

        private const String ServerMethodName = "server.trackS2S";

        public static bool SendRequest(String httpURL, String token, Batch batch)
        {
            var @params = new Dictionary<String, String>(2);
            @params.Add("method", ServerMethodName);
            @params.Add("token", token);
            @params.Add("timestamp", Convert.ToString(Utils.GetNowUnixTimestamp()));

            byte[] deflatedBatch;
            var serializedBatch = Utils.SerializeBatch(batch);
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal))
                {
                    Utils.WriteData(deflateStream, serializedBatch);
                }

                deflatedBatch = memoryStream.ToArray();
            }
            
            var request = (HttpWebRequest)WebRequest.Create(httpURL + "?" + MakeQueryString(@params));
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = deflatedBatch.Length;

            Log.DebugFormat("Getting request (contentLength = {0}) stream for batch with id={1}", deflatedBatch.Length, batch.GetBatchId());
            using (var stream = request.GetRequestStream())
            {
                Log.DebugFormat("Request stream created for batch with id={0}", batch.GetBatchId());
                Log.DebugFormat("Write bytes to stream. Batch id={0}", batch.GetBatchId());
                Utils.WriteData(stream, deflatedBatch);
            }

            try
            {
                Log.DebugFormat("Getting response after sending batch with id={0}", batch.GetBatchId());
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    Log.DebugFormat("Response received for batch with id={0}", batch.GetBatchId());

                    var serializer = new DataContractJsonSerializer(typeof (JsonResponseWrapper));
                    var jsonResponse = (JsonResponseWrapper) serializer.ReadObject(response.GetResponseStream());

                    if (jsonResponse.Error != null)
                    {
                        Log.ErrorFormat("Server return error with message: {0}", jsonResponse.Error.Message);
                    }
                    else if (jsonResponse.Response != null && "OK".Equals(jsonResponse.Response.Status))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Send error", e);
                request.Abort();
            }

            return false;
        }

        private static String MakeQueryString(Dictionary<String, String> @params)
        {
            StringBuilder queryBuilder = new StringBuilder();

            int paramCount = 0;
            foreach (KeyValuePair<string, string> param in @params)
            {
                if (param.Value != null)
                {
                    paramCount++;
                    if (paramCount > 1)
                    {
                        queryBuilder.Append("&");
                    }

                    queryBuilder.Append(param.Key).Append("=").Append(HttpUtility.UrlEncode(param.Value, Encoding.UTF8));
                }
            }
            return queryBuilder.ToString();
        }
    }

    [DataContract]
    [KnownType(typeof (ErrorWrapper))]
    [KnownType(typeof (ResponseWrapper))]
    internal class JsonResponseWrapper
    {
        [DataMember(Name = "error")] public ErrorWrapper Error;
        [DataMember(Name = "response")] public ResponseWrapper Response;
    }

    [DataContract]
    internal class ErrorWrapper
    {
        [DataMember(Name = "message", IsRequired = true)] public String Message;
    }

    [DataContract]
    internal class ResponseWrapper
    {
        [DataMember(Name = "status", IsRequired = true)] public String Status;
    }
}