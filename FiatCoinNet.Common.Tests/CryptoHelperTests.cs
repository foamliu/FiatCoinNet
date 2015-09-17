using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiatCoinNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Common.Tests
{
    [TestClass()]
    public class CryptoHelperTests
    {
        [TestMethod()]
        public void CryptoFunctionTest()
        {
            string privateKey;
            string publicKey;
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);

            string message = "message";
            string signature = CryptoHelper.Sign(privateKey, message);

            bool result = CryptoHelper.Verify(publicKey, message, signature);
            Assert.IsTrue(result);
        }
    }
}