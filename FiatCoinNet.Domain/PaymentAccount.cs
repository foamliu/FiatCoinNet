using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class PaymentAccount
    {
        #region Required Fields
        /// <summary>
        /// An issuer ID (like BIN) + a public key fingerprint
        /// </summary>
        [DataMember]
        public string Address { get; set; }

        /// <summary>
        /// Balance
        /// </summary>
        [DataMember]
        public decimal Balance { get; set; }

        /// <summary>
        /// CurrencyCode
        /// </summary>
        [DataMember]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [DataMember]
        public AccountType Type { get; set; }

        /// <summary>
        /// Public key in base64
        /// </summary>
        [DataMember]
        public string PublicKey { get; set; }

        /// <summary>
        /// Private key in base64
        /// </summary>
        [IgnoreDataMember]
        public string PrivateKey { get; set; }
        #endregion
    }
}
