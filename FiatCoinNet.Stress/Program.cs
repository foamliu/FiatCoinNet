using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using FiatCoinNet.Domain.Requests;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiatCoinNet.Stress
{
    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        private const string baseUrl = "http://fiatcoinet.azurewebsites.net/";
        //private const string baseUrl = "http://localhost:48701/";

        private const int NumberOfAccounts = 10000;
        private const int NumberOfTransactions = 1000000;

        [ThreadStatic]
        public static HttpClient HttpClient = null;
                public static HttpClient GetClient()
        {
            if (HttpClient == null)
            {
                HttpClient = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl),
                };
            }
            return HttpClient;
        }

        static Program()
        {
            DOMConfigurator.Configure();
        }

        static void Main(string[] args)
        {
            logger.Info("prepare issuers");
            HttpResponseMessage response = GetClient().GetAsync("certifier/api/issuers").Result;
            response.EnsureSuccessStatusCode();
            var jsonString = response.Content.ReadAsStringAsync();
            List<Issuer> issuers = JsonConvert.DeserializeObject<List<Issuer>>(jsonString.Result);

            int count = issuers.Count;

            logger.Info("create accounts");
            List<PaymentAccount> accounts = new List<PaymentAccount>();
            for (int i = 1; i <= NumberOfAccounts; i++)
            {
                accounts.Add(new PaymentAccount());
            }

            Parallel.ForEach(accounts, account =>
            {
                int index = (new Random()).Next() % count;
                int issuerId = issuers[index].Id;
                string privateKey, publicKey;
                CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);
                string fingerPrint = CryptoHelper.Hash(publicKey);
                string address = FiatCoinHelper.ToAddress(issuerId, fingerPrint);
                account.Address = address;
                account.CurrencyCode = "USD";
                account.PublicKey = publicKey;
                account.PrivateKey = privateKey;
            });

            logger.Info("register accounts");
            Parallel.ForEach(accounts, account =>
            {
                try
                {
                    int issuerId = FiatCoinHelper.GetIssuerId(account.Address);
                    string requestUri = string.Format("issuer/api/{0}/accounts/register", issuerId);
                    var registerRequest = new RegisterRequest
                    {
                        PaymentAccount = account
                    };
                    HttpContent content = new StringContent(JsonHelper.Serialize(registerRequest));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = GetClient().PostAsync(requestUri, content).Result;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            });

            logger.Info("fund accounts");
            Parallel.ForEach(accounts, account =>
            {
                try
                {
                    int issuerId = FiatCoinHelper.GetIssuerId(account.Address);
                    string requestUri = string.Format("issuer/api/{0}/accounts/fund", issuerId);
                    var fundRequest = new FundRequest
                    {
                        PaymentTransaction = new PaymentTransaction
                        {
                            Source = FiatCoinHelper.EncodeIssuerId(issuerId),
                            Dest = account.Address,
                            Amount = Convert.ToDecimal((new Random()).NextDouble()) * 100.00m,
                            CurrencyCode = "USD",
                            MemoData = "fund with CC"
                        }
                    };
                    HttpContent content = new StringContent(JsonHelper.Serialize(fundRequest));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = GetClient().PostAsync(requestUri, content).Result;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            });

            logger.Info("direct pay");
            var result = Stress.DoWork(() =>
            {
                var transaction = new PaymentTransaction
                {
                    Amount = Convert.ToDecimal((new Random()).NextDouble()) * 20.00m,
                    CurrencyCode = "USD",
                    Source = accounts[(new Random()).Next() % NumberOfAccounts].Address,
                    Dest = accounts[(new Random()).Next() % NumberOfAccounts].Address,
                    MemoData = "TODO: RANDOM STRING"
                };

                int issuerId = FiatCoinHelper.GetIssuerId(transaction.Source);
                string requestUri = string.Format("issuer/api/{0}/accounts/pay", issuerId);
                var payRequest = new DirectPayRequest
                {
                    PaymentTransaction = transaction
                };
                var account = accounts.FirstOrDefault<PaymentAccount>(acct => acct.Address == transaction.Source);
                payRequest.Signature = CryptoHelper.Sign(account.PrivateKey, payRequest.ToMessage());
                HttpContent content = new StringContent(JsonHelper.Serialize(payRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = GetClient().PostAsync(requestUri, content).Result;
            });

            logger.Info(result);
        }
    }
}
