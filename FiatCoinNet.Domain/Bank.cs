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

        [DataMember]
        public List<HigherLevelBlock> s_Blocks { get; set; }

        [DataMember]
        public Queue<LowerLevelBlock> s_LowerLevelBlockPool { get; set; }

        [DataMember]
        public List<Issuer> CertifiedIssuers { get; set; }

        [DataMember]
        public string prevBlockHash { get; set; }

        [DataMember]
        public List<Tuple<long, string>> s_Hashes { get; set; }

        [DataMember]
        public long s_Period { get; set; }

        [DataMember]
        public int version { get; set; }

        public Bank()
        {
            s_Blocks = new List<HigherLevelBlock>();
            s_LowerLevelBlockPool = new Queue<LowerLevelBlock>();
            CertifiedIssuers = new List<Issuer>();
            s_Period = 0;
            version = 1;
        }

        public Bank(string publickey, string privatekey)
        {
            PublicKey = publickey;
            PrivateKey = privatekey;
            s_Blocks = new List<HigherLevelBlock>();
            s_LowerLevelBlockPool = new Queue<LowerLevelBlock>();
            CertifiedIssuers = new List<Issuer>();
            s_Period = 0;
            version = 1;
        }
    }
}
