using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain.Requests
{
    [DataContract]
    public class RegisterRequest : BaseRequest
    {
        /// <summary>
        /// Payment Account
        /// </summary>
        [DataMember]
        public PaymentAccount PaymentAccount { get; set; }
    }
}
