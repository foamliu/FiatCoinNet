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
        public string Hash { get; set; }

        [DataMember]
        public int blockSize { get; set; }

        [DataMember]
        public BlockHeader blockHeader { get; set; }

        [DataMember]
        public int LowLevelBlockCounter { get; set; }

        [DataMember]
        public List<LowerLevelBlock> LowerLevelBlockSet { get; set; }

        //Above are included in bitcoin official doc

        [DataMember]
        public long Period { get; set; }

        [DataMember]
        public string Signature { get; set; }

        public HigherLevelBlock()
        {
            LowerLevelBlockSet = new List<LowerLevelBlock>();
            blockHeader = new BlockHeader();
        }
    }
}
