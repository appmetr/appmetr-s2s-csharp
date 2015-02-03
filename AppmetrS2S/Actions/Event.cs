namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion
    
    [DataContract]
    public class Event : AppMetrAction
    {
        private const String ACTION = "trackEvent";

        [DataMember(Name = "event")]
        private String _event;

        protected Event()
        {
        }

        public Event(string eventName) : base(ACTION)
        {
            _event = eventName;
        }

        public String GetEvent()
        {
            return _event;
        }

        public override int CalcApproximateSize()
        {
            return base.CalcApproximateSize() + GetStringLength(_event);
        }
    }
}