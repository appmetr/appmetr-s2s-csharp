using Newtonsoft.Json;

namespace AppmetrS2S.Actions
{
  
    public class Event : AppMetrAction
    {
        public const string ACTION = "trackEvent";

        [JsonProperty("event")]
        private string _event;

        protected Event()
        {
        }

        public Event(string eventName) : base(ACTION)
        {
            _event = eventName;
        }

        public string GetEvent()
        {
            return _event;
        }

        public override int CalcApproximateSize()
        {
            return base.CalcApproximateSize() + GetStringLength(_event);
        }
    }
}