using System.Runtime.Serialization;

namespace FiatCoinNet.Domain.Requests
{
    [DataContract]
    public class FundRequest : BaseRequest
    {
        /// <summary>
        /// Payment Transaction
        /// </summary>
        [DataMember]
        public PaymentTransaction PaymentTransaction { get; set; }
    }
}