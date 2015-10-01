using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiatCoinNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using FiatCoinNet.Domain;
using Newtonsoft.Json;
using System.Net;
using FiatCoinNet.Domain.Requests;
using System.Net.Http.Headers;

namespace FiatCoinNet.Common.Tests
{
    [TestClass()]
    public class RestApiTests
    {
        private const string baseUrl = "http://fiatcoinet.azurewebsites.net/";
        //private const string baseUrl = "http://localhost:48701/";
        public static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
        };

        [TestMethod()]
        public void CertifierControllerTest()
        {
            HttpResponseMessage response = HttpClient.GetAsync("certifier/api/issuers").Result;
            response.EnsureSuccessStatusCode();
            var jsonString = response.Content.ReadAsStringAsync();
            List<Issuer> issuers = JsonConvert.DeserializeObject<List<Issuer>>(jsonString.Result);

            Assert.AreEqual(2, issuers.Count());
            Assert.AreEqual("Microsoft", issuers[0].Name);
            Assert.AreEqual(1942, issuers[1].Id);

            response = HttpClient.GetAsync("certifier/api/issuers/1942/key").Result;
            response.EnsureSuccessStatusCode();
            jsonString = response.Content.ReadAsStringAsync();
            var key = JsonConvert.DeserializeObject<string>(jsonString.Result);
            Assert.AreEqual("RUNTMSAAAADn7HBQqfSnjcD2R3UFKyirGIAqk65+NPWMIlX3Ilp95HpZLWt9DqSYowSbCQ1wUienJ9wQ2GEoYKWOEwMF9jl6", key);
        }

        [TestMethod()]
        public void IssuerControllerTest()
        {
            int issuerId = 1942;
            int sourceIssuerId = 1942;
            int destIssuerId = 1010;

            // step1: create two account
            // first
            string privateKey, publicKey;
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);
            string fingerPrint = CryptoHelper.Hash(publicKey);
            string address = FiatCoinHelper.ToAddress(sourceIssuerId, fingerPrint);
            var account = new PaymentAccount
            {
                Address = address,
                IssuerId = sourceIssuerId,
                CurrencyCode = "USD",
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
            // second
            string privateKey2, publicKey2;
            CryptoHelper.GenerateKeyPair(out privateKey2, out publicKey2);
            string fingerPrint2 = CryptoHelper.Hash(publicKey2);
            string address2 = FiatCoinHelper.ToAddress(destIssuerId, fingerPrint2);
            var account2 = new PaymentAccount
            {
                Address = address2,
                IssuerId = destIssuerId,
                CurrencyCode = "USD",
                PublicKey = publicKey,
                PrivateKey = privateKey
            };

            // step2: register
            string requestUri = string.Format("issuer/api/{0}/accounts/register", issuerId);
            var registerRequest = new RegisterRequest
            {
                PaymentAccount = account
            };
            HttpContent content = new StringContent(JsonHelper.Serialize(registerRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            // step3: fund
            requestUri = string.Format("issuer/api/{0}/accounts/fund", issuerId);
            var fundRequest = new FundRequest
            {
                PaymentTransaction = new PaymentTransaction
                {
                    Source = FiatCoinHelper.EncodeIssuerId(issuerId),
                    Dest = address,
                    Amount = 100.00m,
                    CurrencyCode = "USD",
                    MemoData = "fund with CC"
                }
            };
            content = new StringContent(JsonHelper.Serialize(fundRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            // step4: get this account & verify
            requestUri = string.Format("issuer/api/{0}/accounts/get", issuerId);
            var getRequest = new GetAccountRequest
            {
                Address = address
            };
            content = new StringContent(JsonHelper.Serialize(getRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();
            var jsonString = response.Content.ReadAsStringAsync();
            var account3 = JsonConvert.DeserializeObject<PaymentAccount>(jsonString.Result);
            Assert.AreEqual(account.Address, account3.Address);
            Assert.AreEqual(account.PublicKey, account3.PublicKey);
            Assert.AreEqual(100.00m, account3.Balance);

            // step5: direct pay
            requestUri = string.Format("issuer/api/{0}/accounts/pay", issuerId);
            var payRequest = new DirectPayRequest
            {
                PaymentTransaction = new PaymentTransaction
                {
                     Source = address,
                     Dest = address2,
                     Amount = 10.00m,
                     CurrencyCode = "USD",
                     MemoData = "surface"
                }
            };
            payRequest.Signature = CryptoHelper.Sign(privateKey, payRequest.ToMessage());
            content = new StringContent(JsonHelper.Serialize(payRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            // step6: get & verify
            requestUri = string.Format("issuer/api/{0}/accounts/get", issuerId);
            getRequest = new GetAccountRequest
            {
                Address = address
            };
            content = new StringContent(JsonHelper.Serialize(getRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();
            jsonString = response.Content.ReadAsStringAsync();
            account3 = JsonConvert.DeserializeObject<PaymentAccount>(jsonString.Result);
            Assert.AreEqual(90.00m, account3.Balance);

            // step7: delete this account
            requestUri = string.Format("issuer/api/{0}/accounts/unregister", issuerId); 
            var unregisterRequest = new UnregisterRequest
            {
                Address = address
            };
            unregisterRequest.Signature = CryptoHelper.Sign(privateKey, unregisterRequest.ToMessage());
            content = new StringContent(JsonHelper.Serialize(unregisterRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();
            
            // step8: get & verify
            requestUri = string.Format("issuer/api/{0}/accounts/get", issuerId);
            getRequest = new GetAccountRequest
            {
                Address = address
            };
            content = new StringContent(JsonHelper.Serialize(getRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = HttpClient.PostAsync(requestUri, content).Result;
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}