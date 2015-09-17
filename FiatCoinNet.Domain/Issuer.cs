using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class Issuer
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string PublicKey { get; set; }

        [IgnoreDataMember]
        public string PrivateKey { get; set; }

        [DataMember]
        public string SignatureToCertifyIssuer { get; set; }

        [DataMember]
        public List<PaymentAccount> PaymentAccounts { get; set; }
    }
}
