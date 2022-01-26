using AppmetrS2S.Serializations;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;


namespace AppmetrS2S
{
    internal class HttpRequestService
    {
        private static readonly ILog Log = LogManager.GetLogger<HttpRequestService>();

        private static readonly int READ_WRITE_TIMEOUT = 10 * 60 * 1000;
        private static readonly int WHOLE_RQUEST_TIMEOUT = 12 * 60 * 1000;

        private const string ServerMethodName = "server.trackS2S";

        public bool SendRequest(string httpUrl, string token, byte[] batch)
        {
            var @params = new Dictionary<string, string>(3)
            {
                {"method", ServerMethodName},
                {"token", token},
                {"timestamp", Convert.ToString(Utils.GetNowUnixTimestamp())}
            };

            var request = (HttpWebRequest)WebRequest.Create(httpUrl + "?" + MakeQueryString(@params));
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = batch.Length;
            request.Timeout = WHOLE_RQUEST_TIMEOUT;
            request.ReadWriteTimeout = READ_WRITE_TIMEOUT;

            Log.DebugFormat("Getting request (contentLength = {0}) stream for batch.", batch.Length);
            using (var stream = request.GetRequestStream())
            {
                Log.DebugFormat("Request stream created for batch.");
                Log.DebugFormat("Write bytes to stream.");
                Utils.WriteData(stream, batch);
            }

            try
            {
                Log.DebugFormat("Getting response after sending batch");
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    Log.DebugFormat("Response received for batch");

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

                    queryBuilder.Append(param.Key).Append("=").Append(Utils.UrlEncode(param.Value, Encoding.UTF8));
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
