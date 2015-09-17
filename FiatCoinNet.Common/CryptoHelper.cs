using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Common
{
    public static class CryptoHelper
    {
        /// <summary>
        /// Hash function: H(data)
        /// </summary>
        public static string Hash(string message)
        {
            using (SHA256Cng sha256 = new SHA256Cng())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.Unicode.GetBytes(message)));
            }
        }

        public static string Hash(byte[] data)
        {
            using (SHA256Cng sha256 = new SHA256Cng())
            {
                return Convert.ToBase64String(sha256.ComputeHash(data));
            }
        }

        public static void GenerateKeyPair(out string privateKey, out string publicKey)
        {
            using (CngKey k = CngKey.Create(CngAlgorithm.ECDsaP256, null, new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                KeyCreationOptions = CngKeyCreationOptions.MachineKey,
                KeyUsage = CngKeyUsages.AllUsages,
                Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None)
            }))
            {
                privateKey = Convert.ToBase64String(k.Export(CngKeyBlobFormat.EccPrivateBlob));
                publicKey = Convert.ToBase64String(k.Export(CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public static string Sign(string privateKey, string message)
        {
            string signature = null;
            
            using (CngKey k = CngKey.Import(Convert.FromBase64String(privateKey), CngKeyBlobFormat.EccPrivateBlob))
            using (ECDsaCng dsa = new ECDsaCng(k))
            {
                byte[] sigData = dsa.SignData(Encoding.Unicode.GetBytes(message));
                signature = Convert.ToBase64String(sigData);
            }

            return signature;
        }

        public static bool Verify(string publicKey, string message, string signature)
        {
            bool isValid = false;

            using (CngKey k = CngKey.Import(Convert.FromBase64String(publicKey), CngKeyBlobFormat.EccPublicBlob))
            using (ECDsaCng dsa = new ECDsaCng(k))
            {
                byte[] sigData = Convert.FromBase64String(signature);
                isValid = dsa.VerifyData(Encoding.Unicode.GetBytes(message), sigData);
            }

            return isValid;
        }

    }


}
