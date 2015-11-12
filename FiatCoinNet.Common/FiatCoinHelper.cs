using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FiatCoinNet.Domain;

namespace FiatCoinNet.Common
{
    public static class FiatCoinHelper
    {
        public static int GetIssuerId(string address)
        {
            return BitConverter.ToInt32(Convert.FromBase64String(address.Substring(0, 8)), 0);
        }

        public static string EncodeIssuerId(int id)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(id));
        }

        public static string ToAddress(int id, string fingerprint)
        {
            return EncodeIssuerId(id) + fingerprint;
        }

        public static void FromAddress(string address, out int id, out string fingerprint)
        {
            id = GetIssuerId(address);
            fingerprint = address.Substring(8);
        }

        
        public static decimal CalculateBalance(List<PaymentTransaction> journal, string address)
        {
            return
                journal.Where(trx => trx.Dest == address).Sum(trx => trx.Amount) -
                journal.Where(trx => trx.Source == address).Sum(trx => trx.Amount);
        }

        public static PaymentAccount GetIssuerAccountFromKeys(int issuerId, string publicKey, string privateKey)
        {
            string fingerPrint = CryptoHelper.Hash(publicKey);
            string address = FiatCoinHelper.ToAddress(issuerId, fingerPrint);
            return new PaymentAccount
            {
                Address = address,
                CurrencyCode = "USD",
                Type = AccountType.Issuer,
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
        }

        /// <summary>
        /// Create HigherLevelBlock 
        /// </summary>
        /// <param name="period"></param>
        /// <param name="hashPrev"></param>
        /// <param name="txset"></param>
        /// <param name="privateKey">PrivateKey of the bank.</param>
        /// <returns></returns>
        public static HigherLevelBlock CreateHigherLevelBlock(long period, string hashPrev, List<PaymentTransaction> txset, string privateKey)
        {
            string hash = CryptoHelper.Hash(hashPrev + JsonHelper.Serialize(txset));
            List<LowerLevelBlock> lowerLevelBlockSet = new List<LowerLevelBlock>();
            LowerLevelBlock lowerLevelBlock = new LowerLevelBlock(txset);
            lowerLevelBlockSet.Add(lowerLevelBlock);
            return new HigherLevelBlock
            {
                Period = period,
                Hash = hash,
                LowerLevelBlockSet = lowerLevelBlockSet,
                Signature = CryptoHelper.Sign(privateKey, hash)
            };
        }

        /// <summary>
        /// Verify HigherLevelBlock
        /// </summary>
        /// <param name="block"></param>
        /// <param name="hashPrev"></param>
        /// <param name="publicKey">PublicKey of the bank.</param>
        /// <returns></returns>
        public static bool VerifyHigherLevelBlock(HigherLevelBlock block, string hashPrev, string publicKey)
        {
            string hash = CryptoHelper.Hash(hashPrev + JsonHelper.Serialize(block.LowerLevelBlockSet[0].TransactionSet));

            if (block.Hash == hash)
            {
                return CryptoHelper.Verify(publicKey, hash, block.Signature);
            }

            return false;
        }

        /// <summary>
        /// Create LowerLevelBlock
        /// </summary>
        /// <param name="period"></param>
        /// <param name="hashPrev"></param>
        /// <param name="txset"></param>
        /// <param name="privateKey"></param>
        /// <param name="issuerPublicKey"></param>
        /// <returns></returns>
        public static LowerLevelBlock CreateLowerLevelBlock(long period, string hashPrev, List<PaymentTransaction> txset, string issuerPrivateKey, string signatureToCertifyIssuer)
        {
            string hash = CryptoHelper.Hash(JsonHelper.Serialize(txset)); //TODO(foamliu):fix it.

            return new LowerLevelBlock
            {
                Period = period,
                Hash = hash,
                TransactionSet = txset,
                Signature = CryptoHelper.Sign(issuerPrivateKey, hash),
                SignatureToCertifyIssuer = signatureToCertifyIssuer,
            };
        }

        /// <summary>
        /// Verify LowerLevelBlock
        /// </summary>
        /// <param name="block"></param>
        /// <param name="hashPrev"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static bool VerifyLowerLevelBlock(LowerLevelBlock block, string hashPrev, string publicKey, string issuerPublicKey)
        {
            string hash = CryptoHelper.Hash(JsonHelper.Serialize(block.TransactionSet));

            if (block.Hash == hash)
            {
                if (CryptoHelper.Verify(issuerPublicKey, hash, block.Signature))
                {
                    if (CryptoHelper.Verify(publicKey, issuerPublicKey, block.SignatureToCertifyIssuer))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    
        public static bool CheckNotDoubleSpent()
        {
            return true;
        }
    }
}
