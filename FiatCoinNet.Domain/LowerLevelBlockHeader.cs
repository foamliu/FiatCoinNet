using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class LowerLevelBlockHeader
    {
        #region Required Fields
        /// <summary>
        /// identify the block belongs to which issuer
        /// </summary>
        [DataMember]
        public string Issuer { get; set; }

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
        public double Time { get; set; }

        /// <summary>
        /// Current target in compact format
        /// </summary>
        [DataMember]
        public int Bits { get; set; }

        /// <summary>
        /// Block version number
        /// </summary>
        [DataMember]
        public int Version { get; set; }

        #endregion
    }
}
