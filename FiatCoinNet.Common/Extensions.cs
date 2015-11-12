using FiatCoinNet.Domain.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Common
{
    public static class Extensions
    {
        public static string ToMessage(this BaseRequest request)
        {
            return JsonHelper.Serialize(request);
        }
    }
    
}
