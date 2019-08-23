namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public class AttachProperties : AppMetrAction
    {
        public const String ACTION = "attachProperties";

        public AttachProperties() : base(ACTION)
        {
        }
    }
}
