using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class BlockHeader
    {
        #region Required Fields
        /// <summary>
        /// Block version number
        /// </summary>
        [DataMember]
        public int Version { get; set; }

        /// <summary>
        /// 256-bit hash of the previous block header
        /// </summary>
        [DataMember]
        public string hashPrevBlock { get; set; }

        /// <summary>
        /// 256-bit hash based on all of the transactions in the block
        /// </summary>
        [DataMember]
        public string hashMerkleRoot { get; set; }

        /// <summary>
        /// Current timestamp as seconds since 1970-01-01T00:00 UTC
        /// </summary>
        [DataMember]
        public int Time { get; set; }

        /// <summary>
        /// Current target in compact format
        /// </summary>
        [DataMember]
        public int Bits { get; set; }

        /// <summary>
        /// 32-bit number (starts at 0)
        /// </summary>
        [DataMember]
        public int Nonce = 0;

        #endregion
    }
}
