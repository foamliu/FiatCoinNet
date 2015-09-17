using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain.Requests
{
    [DataContract]
    public class DirectPayRequest : BaseRequest
    {
        /// <summary>
        /// Payment Transaction
        /// </summary>
        [DataMember]
        public PaymentTransaction PaymentTransaction { get; set; }
    }
}
