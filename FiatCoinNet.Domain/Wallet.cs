using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class Wallet
    {
        [DataMember]
        public List<PaymentAccount> PaymentAccounts { get; set; }

        public Wallet()
        {
            PaymentAccounts = new List<PaymentAccount>();
        }
    }
}
