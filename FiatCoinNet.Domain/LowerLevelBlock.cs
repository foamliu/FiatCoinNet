using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class LowerLevelBlock
    {
        [DataMember]
        public long Period { get; set; }

        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public List<PaymentTransaction> TransactionSet { get; set; }

        [DataMember]
        public string Signature { get; set; }

        [DataMember]
        public string SignatureToCertifyIssuer { get; set; }

        public LowerLevelBlock()
        {
            TransactionSet = new List<PaymentTransaction>();
        }
    }
}
