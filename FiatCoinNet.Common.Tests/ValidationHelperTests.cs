using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiatCoinNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiatCoinNet.Domain.Requests;
using FiatCoinNet.Domain;

namespace FiatCoinNet.Common.Tests
{
    [TestClass()]
    public class ValidationHelperTests
    {
        [TestMethod()]
        public void ValidateTest()
        {
            int issuerId = 1942;

            // step1: create two account
            // first
            string privateKey, publicKey;
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);
            string fingerPrint = CryptoHelper.Hash(publicKey);
            string address = FiatCoinHelper.ToAddress(issuerId, fingerPrint);
            var account = new PaymentAccount
            {
                Address = address,
                IssuerId = issuerId,
                CurrencyCode = "USD",
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
            // second
            string privateKey2, publicKey2;
            CryptoHelper.GenerateKeyPair(out privateKey2, out publicKey2);
            string fingerPrint2 = CryptoHelper.Hash(publicKey2);
            string address2 = FiatCoinHelper.ToAddress(issuerId, fingerPrint2);
            var account2 = new PaymentAccount
            {
                Address = address2,
                IssuerId = issuerId,
                CurrencyCode = "CNY",
                PublicKey = publicKey,
                PrivateKey = privateKey
            };

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

            bool authorized = ValidationHelper.Validate(payRequest, publicKey);
            Assert.IsTrue(authorized);

            payRequest.PaymentTransaction.Dest = "Bad man";
            payRequest.PaymentTransaction.Amount = 10000.00m;
            payRequest.Timestamp = DateTime.Parse("2016-01-01");
            authorized = ValidationHelper.Validate(payRequest, publicKey);
            Assert.IsFalse(authorized);
        }
    }
}