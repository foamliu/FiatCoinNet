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
        /// <summary>
        /// hash refers to block id
        /// </summary>
        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public int blockSize { get; set; }

        [DataMember]
        public LowerLevelBlockHeader blockHeader { get; set; }

        [DataMember]
        public int TransactionCounter { get; set; }

        [DataMember]
        public List<PaymentTransaction> TransactionSet { get; set; }

        //Above are included in bitcoin official doc

        [DataMember]
        public long Period { get; set; }

        [DataMember]
        public string Signature { get; set; }

        [DataMember]
        public string SignatureToCertifyIssuer { get; set; }

        public LowerLevelBlock()
        {
            TransactionSet = new List<PaymentTransaction>();
            blockHeader = new LowerLevelBlockHeader();
        }

        public LowerLevelBlock(List<PaymentTransaction> transactionSet)
        {
            TransactionSet = transactionSet;
            blockHeader = new LowerLevelBlockHeader();
        }
    }
}
