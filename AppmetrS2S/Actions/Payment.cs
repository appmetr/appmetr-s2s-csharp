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

        [DataMember(Name = "appCurrencyCode")]
        private String _appCurrencyCode;

        [DataMember(Name = "appCurrencyAmount")]
        private String _appCurrencyAmount;

        protected Payment()
        {
        }

        public Payment(String orderId,
            String transactionId,
            String processor,
            String psUserSpentCurrencyCode,
            String psUserSpentCurrencyAmount)
            : this(orderId, transactionId, processor, psUserSpentCurrencyCode, psUserSpentCurrencyAmount, null, null)
        {
        }

        public Payment(String orderId,
            String transactionId,
            String processor,
            String psUserSpentCurrencyCode,
            String psUserSpentCurrencyAmount,
            String appCurrencyCode,
            String appCurrencyAmount) : base(ACTION)
        {
            _orderId = orderId;
            _transactionId = transactionId;
            _processor = processor;
            _psUserSpentCurrencyCode = psUserSpentCurrencyCode;
            _psUserSpentCurrencyAmount = psUserSpentCurrencyAmount;
            _appCurrencyCode = appCurrencyCode;
            _appCurrencyAmount = appCurrencyAmount;
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

        public String GetAppCurrencyCode()
        {
            return _appCurrencyCode;
        }

        public String GetAppCurrencyAmount()
        {
            return _appCurrencyAmount;
        }

        public override int CalcApproximateSize()
        {
            return base.CalcApproximateSize()
                   + GetStringLength(_orderId)
                   + GetStringLength(_transactionId)
                   + GetStringLength(_processor)
                   + GetStringLength(_psUserSpentCurrencyCode)
                   + GetStringLength(_psUserSpentCurrencyAmount)
                   + GetStringLength(_appCurrencyCode)
                   + GetStringLength(_appCurrencyAmount);
        }
    }
}