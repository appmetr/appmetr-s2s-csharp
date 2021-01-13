using Newtonsoft.Json;

namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Collections.Generic;

    #endregion

    public abstract class AppMetrAction
    {
        [JsonProperty("action")]
        private string _action;

        [JsonProperty("timestamp")]
        private long _timestamp = Utils.GetNowUnixTimestamp();

        [JsonProperty("userTime")]
        private long? _userTime;

        [JsonProperty("properties")]
        private IDictionary<string, object> _properties = new Dictionary<string, object>();

        [JsonProperty("userId")]
        private string _userId;

        protected AppMetrAction()
        {
        }

        protected AppMetrAction(string action)
        {
            _action = action;
        }

        public long GetTimestamp()
        {
            return _userTime ?? _timestamp; ;
        }

        public AppMetrAction SetTimestamp(long timestamp)
        {
            _userTime = timestamp;
            return this;
        }

        public IDictionary<string, object> GetProperties()
        {
            return _properties;
        }

        public AppMetrAction SetProperties(IDictionary<string, object> properties)
        {
            _properties = properties;
            return this;
        }

        public string GetUserId()
        {
            return _userId;
        }

        public AppMetrAction SetUserId(string userId)
        {
            _userId = userId;
            return this;
        }

        //http://codeblog.jonskeet.uk/2011/04/05/of-memory-and-strings/
        public virtual int CalcApproximateSize()
        {
            var size = 40 + (40 * _properties.Count); //40 - Map size and 40 - each entry overhead

            size += GetStringLength(_action);
            size += GetStringLength(Convert.ToString(_timestamp));
            size += GetStringLength(_userId);

            foreach (var pair in _properties) {
                size += GetStringLength(pair.Key);
                size += GetStringLength(pair.Value != null ? Convert.ToString(pair.Value) : null);   //toString because sending this object via json
            }

            return 8 + size + 8; //8 - object header
        }

        protected int GetStringLength(string str)
        {
            return str?.Length * 2 + 26 ?? 0;    //24 - String object size, 16 - char[]
        }
    }
}