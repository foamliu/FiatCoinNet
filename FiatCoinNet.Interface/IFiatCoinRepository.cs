using FiatCoinNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Interface
{
    public interface IFiatCoinRepository
    {
        PaymentAccount AddAccount(PaymentAccount newAccount);

        PaymentAccount GetAccount(string address);

        List<PaymentAccount> GetAccounts(int issuerId);

        void CloseAccount(string address);

        PaymentTransaction AddTransaction(PaymentTransaction newTransaction);

        List<PaymentTransaction> GetTransactions(string address);
    }
}
