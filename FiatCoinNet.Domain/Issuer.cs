using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FiatCoinNet.Domain.Requests;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

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

        [DataMember]
        public int version { get; set; }

        [DataMember]
        public int epoch { get; set; }

        [DataMember]
        public Queue<PaymentTransaction> s_PaymentPool { get; set; }

        [DataMember]
        public ConcurrentDictionary<int, List<LowerLevelBlock>> s_Blocks { get; set; }

        [DataMember]
        public const int MAX_TRANSACTION = 100;

        /*
        private const int MAX_TRANSACTION = 100;
        public static readonly ConcurrentDictionary<int, List<LowerLevelBlock>> s_Blocks;
        public static readonly Queue<PaymentTransaction> s_PaymentPool;
        public static int epoch;
        */

        public Issuer()
        {
            epoch = 60000;
            version = 1;
            PaymentAccounts = new List<PaymentAccount>();
            s_Blocks = new ConcurrentDictionary<int, List<LowerLevelBlock>>();
            s_PaymentPool = new Queue<PaymentTransaction>();
        }

        public Issuer(int Id, string Name, string PublicKey, string PrivateKey, string SignatureToCertifyIssuer)
        {
            this.Id = Id;
            this.Name = Name;
            this.PublicKey = PublicKey;
            this.PrivateKey = PrivateKey;
            this.SignatureToCertifyIssuer = SignatureToCertifyIssuer;
            epoch = 60000;
            version = 1;
            PaymentAccounts = new List<PaymentAccount>();
            s_Blocks = new ConcurrentDictionary<int, List<LowerLevelBlock>>();
            s_PaymentPool = new Queue<PaymentTransaction>();
        }

        


    }
}
