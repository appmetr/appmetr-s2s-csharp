namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public class AddToPropertiesValue : AppMetrAction
    {
        private const String ACTION = "addToPropertiesValue";

        public AddToPropertiesValue() : base(ACTION)
        {
        }
    }
}
