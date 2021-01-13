using Newtonsoft.Json;

namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion
    
    public class AttachEntityAttributes : AppMetrAction
    {
        public const string ACTION = "attachEntityAttributes";

        [JsonProperty("entityName")]
        private string _entityName;

        [JsonProperty("entityValue")]
        private string _entityValue;

        protected AttachEntityAttributes()
        {
        }

        public AttachEntityAttributes(string entityName, string entityValue) : base(ACTION)
        {
            _entityName = entityName;
            _entityValue = entityValue;
        }

        public string GetEntityName()
        {
            return _entityName;
        }

        public string GetEntityValue()
        {
            return _entityValue;
        }

        public override int CalcApproximateSize()
        {
            return base.CalcApproximateSize() + GetStringLength(_entityName) + GetStringLength(_entityValue);
        }
    }
}