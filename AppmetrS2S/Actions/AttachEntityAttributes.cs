namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion
    
    [DataContract]
    public class AttachEntityAttributes : AppMetrAction
    {
        public const String ACTION = "attachEntityAttributes";

        [DataMember(Name = "entityName")]
        private String _entityName;

        [DataMember(Name = "entityValue")]
        private String _entityValue;

        protected AttachEntityAttributes()
        {
        }

        public AttachEntityAttributes(String entityName, String entityValue) : base(ACTION)
        {
            _entityName = entityName;
            _entityValue = entityValue;
        }

        public String GetEntityName()
        {
            return _entityName;
        }

        public String GetEntityValue()
        {
            return _entityValue;
        }

        public override int CalcApproximateSize()
        {
            return base.CalcApproximateSize() + GetStringLength(_entityName) + GetStringLength(_entityValue);
        }
    }
}