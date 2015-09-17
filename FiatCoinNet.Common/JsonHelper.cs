using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Common
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
