using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain.Requests
{
    [DataContract]
    public abstract class BaseRequest
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        [DataMember]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Signature
        /// </summary>
        [DataMember]
        public string Signature { get; set; }

        public BaseRequest()
        {
            this.Timestamp = DateTime.UtcNow;
        }
    }
}
