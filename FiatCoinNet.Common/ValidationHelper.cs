using FiatCoinNet.Domain.Requests;
using System;

namespace FiatCoinNet.Common
{
    public static class ValidationHelper
    {
        public static bool Validate(BaseRequest request, string publicKey)
        {
            string signature = request.Signature;
            request.Signature = null;
            string message = JsonHelper.Serialize(request);
            request.Signature = signature;
            return CryptoHelper.Verify(publicKey, message, signature);
        }
    }
}
