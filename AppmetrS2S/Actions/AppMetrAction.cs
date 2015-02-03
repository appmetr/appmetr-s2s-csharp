namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public abstract class AppMetrAction
    {
        [DataMember(Name = "action")]
        private String _action;

        [DataMember(Name = "timestamp")]
        private long _timestamp = Utils.GetNowUnixTimestamp();

        [DataMember(Name = "properties")]
        private IDictionary<String, Object> _properties = new Dictionary<string, object>();

        [DataMember(Name = "userId")]
        private String _userId;

        protected AppMetrAction()
        {
        }

        protected AppMetrAction(string action)
        {
            _action = action;
        }

        public long GetTimestamp()
        {
            return _timestamp;
        }

        public AppMetrAction SetTimestamp(long timestamp)
        {
            _timestamp = timestamp;
            return this;
        }

        public IDictionary<String, Object> GetProperties()
        {
            return _properties;
        }

        public AppMetrAction SetProperties(IDictionary<String, Object> properties)
        {
            _properties = properties;
            return this;
        }

        public String GetUserId()
        {
            return _userId;
        }

        public AppMetrAction SetUserId(String userId)
        {
            _userId = userId;
            return this;
        }

        //http://codeblog.jonskeet.uk/2011/04/05/of-memory-and-strings/
        public virtual int CalcApproximateSize()
        {
            int size = 40 + (40 * _properties.Count); //40 - Map size and 40 - each entry overhead

            size += GetStringLength(_action);
            size += GetStringLength(Convert.ToString(_timestamp));
            size += GetStringLength(_userId);

            foreach (KeyValuePair<String, Object> pair in _properties) {
                size += GetStringLength(pair.Key);
                size += GetStringLength(pair.Value != null ? Convert.ToString(pair.Value) : null);   //toString because sending this object via json
            }

            return 8 + size + 8; //8 - object header
        }

        protected int GetStringLength(String str)
        {
            return str == null ? 0 : str.Length * 2 + 26;    //24 - String object size, 16 - char[]
        }
    }
}