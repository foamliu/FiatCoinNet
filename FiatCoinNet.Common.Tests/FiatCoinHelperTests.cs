using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiatCoinNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiatCoinNet.Domain;

namespace FiatCoinNet.Common.Tests
{
    [TestClass()]
    public class FiatCoinHelperTests
    {
        [TestMethod()]
        public void FunctionTest()
        {
            int id, id2;
            string privateKey, publicKey;
            string fingerPrint, fingerPrint2;
            string address;

            id = (new Random()).Next();
            string encodedID = Convert.ToBase64String(BitConverter.GetBytes(id));
            Assert.AreEqual(8, encodedID.Length);

            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);
            fingerPrint = CryptoHelper.Hash(publicKey);
            address = FiatCoinHelper.ToAddress(id, fingerPrint);
            FiatCoinHelper.FromAddress(address, out id2, out fingerPrint2);

            Assert.AreEqual(id, id2);
            Assert.AreEqual(fingerPrint, fingerPrint2);
        }

        [TestMethod()]
        public void CalculateBalanceTest()
        {
            int issuerId = 1942;
            // step1: create three account
            // first
            string privateKey, publicKey;
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);
            string fingerPrint = CryptoHelper.Hash(publicKey);
            string address = FiatCoinHelper.ToAddress(issuerId, fingerPrint);
            var account = new PaymentAccount
            {
                Address = address,
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
                CurrencyCode = "CNY",
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
            // third
            string privateKey3, publicKey3;
            CryptoHelper.GenerateKeyPair(out privateKey3, out publicKey3);
            string fingerPrint3 = CryptoHelper.Hash(publicKey3);
            string address3 = FiatCoinHelper.ToAddress(issuerId, fingerPrint3);
            var account3 = new PaymentAccount
            {
                Address = address2,
                CurrencyCode = "CNY",
                PublicKey = publicKey,
                PrivateKey = privateKey
            };

            // step2 create two transactions
            var trx1 = new PaymentTransaction
            {
                Source = address,
                Dest = address2,
                Amount = 100.00m,
                CurrencyCode = "USD",
                MemoData = "surface"
            };
            var trx2 = new PaymentTransaction
            {
                Source = address2,
                Dest = address3,
                Amount = 55.00m,
                CurrencyCode = "USD",
                MemoData = "surface"
            };
            var journal = new List<PaymentTransaction>();
            journal.Add(trx1);
            journal.Add(trx2);

            // step3 calculate & verify
            var balance = FiatCoinHelper.CalculateBalance(journal, address2);

            Assert.AreEqual(45m, balance);
        }

        [TestMethod()]
        public void HigherLevelBlockTest()
        {
            string privateKey, publicKey; // bank's
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);

            long period = (new Random()).Next();
            string hashPrev = CryptoHelper.Hash(string.Empty);
            List<PaymentTransaction> txset = new List<PaymentTransaction>();
            txset.Add(new PaymentTransaction
            {
                Amount = Convert.ToDecimal((new Random()).NextDouble()) * 20.00m,
                CurrencyCode = "USD",
                Source = "address1",
                Dest = "address2",
                MemoData = "TODO: RANDOM STRING"
            });
            txset.Add(new PaymentTransaction
            {
                Amount = Convert.ToDecimal((new Random()).NextDouble()) * 20.00m,
                CurrencyCode = "USD",
                Source = "address2",
                Dest = "address3",
                MemoData = "TODO: RANDOM STRING"
            });

            var block = FiatCoinHelper.CreateHigherLevelBlock(
                period,
                hashPrev,
                txset,
                privateKey);

            bool valid = FiatCoinHelper.VerifyHigherLevelBlock(block, hashPrev, publicKey);
            Assert.IsTrue(valid);
        }

        [TestMethod()]
        public void LowerLevelBlockTest()
        {
            string privateKey, publicKey; // bank's
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);
            string issuerPrivateKey, issuerPublicKey; // issuer's
            CryptoHelper.GenerateKeyPair(out issuerPrivateKey, out issuerPublicKey);

            long period = (new Random()).Next();
            string hashPrev = CryptoHelper.Hash(string.Empty);
            List<PaymentTransaction> txset = new List<PaymentTransaction>();
            txset.Add(new PaymentTransaction
            {
                Amount = Convert.ToDecimal((new Random()).NextDouble()) * 20.00m,
                CurrencyCode = "USD",
                Source = "address1",
                Dest = "address2",
                MemoData = "TODO: RANDOM STRING"
            });
            txset.Add(new PaymentTransaction
            {
                Amount = Convert.ToDecimal((new Random()).NextDouble()) * 20.00m,
                CurrencyCode = "USD",
                Source = "address2",
                Dest = "address3",
                MemoData = "TODO: RANDOM STRING"
            });

            string signatureToCertifyIssuer = CryptoHelper.Sign(privateKey, issuerPublicKey);

            var block = FiatCoinHelper.CreateLowerLevelBlock(
                period,
                hashPrev,
                txset,
                issuerPrivateKey,
                signatureToCertifyIssuer);

            bool valid = FiatCoinHelper.VerifyLowerLevelBlock(block, hashPrev, publicKey, issuerPublicKey);
            Assert.IsTrue(valid);
        }
    }
}