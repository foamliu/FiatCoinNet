using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FiatCoinNet.Common
{
    public static class DataAccessor
    {
        private static string FileNameCurrencyCodes = @"Data\CurrencyCodes.json";

        private static string GetDirectoryPath()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }

        public static List<string> GetCurrencyCodes()
        {
            string filePath = Path.Combine(GetDirectoryPath(), FileNameCurrencyCodes);
            string json = File.ReadAllText(filePath);
            return JsonHelper.Deserialize<List<string>>(json);
        } 
    }
}
