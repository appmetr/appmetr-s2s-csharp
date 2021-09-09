using Newtonsoft.Json;

namespace AppmetrS2S.Actions
{
    public class Payment : AppMetrAction
    {
        public const string ACTION = "trackPayment";

        [JsonProperty("orderId")]
        private string _orderId;

        [JsonProperty("transactionId")]
        private string _transactionId;

        [JsonProperty("processor")]
        private string _processor;

        [JsonProperty("psUserSpentCurrencyCode")]
        private string _psUserSpentCurrencyCode;

        [JsonProperty("psUserSpentCurrencyAmount")]
        private string _psUserSpentCurrencyAmount;

        [JsonProperty("psReceivedCurrencyCode")]
        public string _psReceivedCurrencyCode;

        [JsonProperty("psReceivedCurrencyAmount")]
        public string _psReceivedCurrencyAmount;

        [JsonProperty("appCurrencyCode")]
        private string _appCurrencyCode;

        [JsonProperty("appCurrencyAmount")]
        private string _appCurrencyAmount;

        [JsonProperty("psUserStoreCountryCode")]
        public string _psUserStoreCountryCode;

        [JsonProperty("isSandbox")]
        public bool? _isSandbox;

        protected Payment() : base(ACTION)
        {
        }

        public Payment(
	        string orderId,
            string transactionId,
            string processor,
            string psUserSpentCurrencyCode,
            string psUserSpentCurrencyAmount,
            string psReceivedCurrencyCode = null,
            string psReceivedCurrencyAmount = null,
            string appCurrencyCode = null,
            string appCurrencyAmount = null,
            string psUserStoreCountryCode = null,
            bool? isSandbox = null
            ) : this()
        {
            _orderId = orderId;
            _transactionId = transactionId;
            _processor = processor;
            _psUserSpentCurrencyCode = psUserSpentCurrencyCode;
            _psUserSpentCurrencyAmount = psUserSpentCurrencyAmount;
            _psReceivedCurrencyCode = psReceivedCurrencyCode;
            _psReceivedCurrencyAmount = psReceivedCurrencyAmount;
            _appCurrencyCode = appCurrencyCode;
            _appCurrencyAmount = appCurrencyAmount;
            _psUserStoreCountryCode = psUserStoreCountryCode;
            _isSandbox = isSandbox;
        }

        public string GetOrderId()
        {
            return _orderId;
        }

        public string GetTransactionId()
        {
            return _transactionId;
        }

        public string GetProcessor()
        {
            return _processor;
        }

        public string GetPsUserSpentCurrencyCode()
        {
            return _psUserSpentCurrencyCode;
        }

        public string GetPsUserSpentCurrencyAmount()
        {
            return _psUserSpentCurrencyAmount;
        }

        public string GetPsReceivedCurrencyCode()
        {
            return _psReceivedCurrencyCode;
        }

        public string GetPsReceivedCurrencyAmount()
        {
            return _psReceivedCurrencyAmount;
        }

        public string GetAppCurrencyCode()
        {
            return _appCurrencyCode;
        }

        public string GetAppCurrencyAmount()
        {
            return _appCurrencyAmount;
        }

        public string GetPsUserStoreCountryCode()
        {
            return _psUserStoreCountryCode;
        }

        public bool? GetIsSandbox()
        {
            return _isSandbox;
        }

        public override int CalcApproximateSize()
        {
            return base.CalcApproximateSize()
                   + GetStringLength(_orderId)
                   + GetStringLength(_transactionId)
                   + GetStringLength(_processor)
                   + GetStringLength(_psUserSpentCurrencyCode)
                   + GetStringLength(_psUserSpentCurrencyAmount)
                   + GetStringLength(_psReceivedCurrencyCode)
                   + GetStringLength(_psReceivedCurrencyAmount)
                   + GetStringLength(_appCurrencyCode)
                   + GetStringLength(_appCurrencyAmount)
                   + GetStringLength(_psUserStoreCountryCode);
        }
    }
}