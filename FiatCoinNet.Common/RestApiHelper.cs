using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNet.Common
{
    public static class RestApiHelper
    {
        //private const string baseUrl = "http://fiatcoinet.azurewebsites.net/";
        private const string baseUrl = "http://localhost:48701/";
        public static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
        };

    }
}
