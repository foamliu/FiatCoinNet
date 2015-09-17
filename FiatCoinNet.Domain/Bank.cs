using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class Bank
    {
        [DataMember]
        public string PublicKey { get; set; }

        [IgnoreDataMember]
        public string PrivateKey { get; set; }
    }
}
