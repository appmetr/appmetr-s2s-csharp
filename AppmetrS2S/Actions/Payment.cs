namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public class Payment : AppMetrAction
    {
        private const String ACTION = "trackPayment";

        [DataMember(Name = "orderId")]
        private String _orderId;

        [DataMember(Name = "transactionId")]
        private String _transactionId;

        [DataMember(Name = "processor")]
        private String _processor;

        [DataMember(Name = "psUserSpentCurrencyCode")]
        private String _psUserSpentCurrencyCode;

        [DataMember(Name = "psUserSpentCurrencyAmount")]
        private String _psUserSpentCurrencyAmount;

        [DataMember(Name = "psReceivedCurrencyCode")]
        public String _psReceivedCurrencyCode;

        [DataMember(Name = "psReceivedCurrencyAmount")]
        public String _psReceivedCurrencyAmount;

        [DataMember(Name = "appCurrencyCode")]
        private String _appCurrencyCode;

        [DataMember(Name = "appCurrencyAmount")]
        private String _appCurrencyAmount;

        [DataMember(Name = "psUserStoreCountryCode")]
        public String _psUserStoreCountryCode;

        [DataMember(Name = "$sandbox")]
        public bool? _isSandbox;

        protected Payment() : base(ACTION)
        {
        }

        public Payment(
            String orderId,
            String transactionId,
            String processor,
            String psUserSpentCurrencyCode,
            String psUserSpentCurrencyAmount,
            String psReceivedCurrencyCode = null,
            String psReceivedCurrencyAmount = null,
            String appCurrencyCode = null,
            String appCurrencyAmount = null,
            String psUserStoreCountryCode = null,
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

        public String GetOrderId()
        {
            return _orderId;
        }

        public String GetTransactionId()
        {
            return _transactionId;
        }

        public String GetProcessor()
        {
            return _processor;
        }

        public String GetPsUserSpentCurrencyCode()
        {
            return _psUserSpentCurrencyCode;
        }

        public String GetPsUserSpentCurrencyAmount()
        {
            return _psUserSpentCurrencyAmount;
        }

        public String GetPsReceivedCurrencyCode()
        {
            return _psReceivedCurrencyCode;
        }

        public String GetPsReceivedCurrencyAmount()
        {
            return _psReceivedCurrencyAmount;
        }

        public String GetAppCurrencyCode()
        {
            return _appCurrencyCode;
        }

        public String GetAppCurrencyAmount()
        {
            return _appCurrencyAmount;
        }

        public String GetPsUserStoreCountryCode()
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