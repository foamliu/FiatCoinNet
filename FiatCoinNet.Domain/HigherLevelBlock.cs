using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class HigherLevelBlock
    {
        [DataMember]
        public long Period { get; set; }

        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public List<PaymentTransaction> TransactionSet { get; set; }

        [DataMember]
        public string Signature { get; set; }
    }
}
