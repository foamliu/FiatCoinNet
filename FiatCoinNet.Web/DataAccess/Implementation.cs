using FiatCoinNet.Domain;
using FiatCoinNet.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNetWeb.DataAccess
{
    public partial class DataAccessor : IFiatCoinRepository
    {
        public PaymentAccount AddAccount(PaymentAccount newAccount)
        {
            var result = QueryStoreProcedure("AddAccount", new Dictionary<string, object>
                                                          {
                                                              {"@address", newAccount.Address},
                                                              {"@issuerId", newAccount.IssuerId},
                                                              {"@currencyCode", newAccount.CurrencyCode},
                                                              {"@publicKey", newAccount.PublicKey},
                                                          });
            if (result.Tables[0].Rows.Count > 0)
            {
                var acct = new PaymentAccount().FromRow(result.Tables[0].Rows[0]);
                return acct;
            }
            return null;
        }

        public PaymentTransaction AddTransaction(PaymentTransaction newTransaction)
        {
            var result = QueryStoreProcedure("AddTransaction", new Dictionary<string, object>
                                                          {
                                                              {"@source", newTransaction.Source},
                                                              {"@dest", newTransaction.Dest},
                                                              {"@amount", newTransaction.Amount},
                                                              {"@currencyCode", newTransaction.CurrencyCode},
                                                              {"@memoData", newTransaction.MemoData},
                                                          });
            if (result.Tables[0].Rows.Count > 0)
            {
                var acct = new PaymentTransaction().FromRow(result.Tables[0].Rows[0]);
                return acct;
            }
            return null;
        }

        public void CloseAccount(string address)
        {
            QueryStoreProcedure("CloseAccount", new Dictionary<string, object>
                                                          {
                                                              {"@address", address},
                                                          });
        }

        public PaymentAccount GetAccount(string address)
        {
            var result = QueryStoreProcedure("GetAccount", new Dictionary<string, object>
                                                          {
                                                              {"@address", address},
                                                          });
            if (result.Tables[0].Rows.Count > 0)
            {
                var acct = new PaymentAccount().FromRow(result.Tables[0].Rows[0]);
                return acct;
            }
            return null;
        }

        public List<PaymentAccount> GetAccounts(int issuerId)
        {
            var list = new List<PaymentAccount>();

            var result = QueryStoreProcedure("GetAccounts", new Dictionary<string, object>
                                                          {
                                                              {"@issuerId", issuerId},
                                                          });
            if (result.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    list.Add(new PaymentAccount().FromRow(row));
                }
            }
            return list;
        }

        public List<PaymentTransaction> GetTransactions(string address)
        {
            var list = new List<PaymentTransaction>();

            var result = QueryStoreProcedure("GetTransactions", new Dictionary<string, object>
                                                          {
                                                              {"@address", address},
                                                          });
            if (result.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    list.Add(new PaymentTransaction().FromRow(row));
                }
            }
            return list;
        }
    }
}
