using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain.Requests
{
    [DataContract]
    public class GetAccountRequest : BaseRequest
    {
        /// <summary>
        /// Address
        /// </summary>
        [DataMember]
        public string Address { get; set; }
    }
}
