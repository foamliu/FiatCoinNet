using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Domain
{
    [DataContract]
    public class PaymentTransaction
    {
        #region Required Fields
        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string Dest { get; set; }
        #endregion

        #region Optional Fields
        [DataMember]
        public string InvoiceID { get; set; }

        [DataMember]
        public string RefPaymentTrxId { get; set; }

        [DataMember]
        public string MemoData { get; set; }
        #endregion

        #region Framework Fields
        [IgnoreDataMember]
        public string TransactionId { get; set; }
        #endregion
    }
}
