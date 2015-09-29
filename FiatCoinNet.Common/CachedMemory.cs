using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FiatCoinNet.Common
{
    public class CachedMemory
    {
        private static readonly ConcurrentDictionary<string, object> Cache = null;
        private static readonly string PersistenceFilePath = HttpContext.Current.Server.MapPath("~/App_Data/CachedMemory.dat");
        private const int Period = 1000 * 60; // 1min
        private const int DueTime = Period; // 1min

        #region singleton instance

        private static CachedMemory _instance;

        public static CachedMemory Instance
        {
            get
            {
                if (_instance == null)
                {
                    Interlocked.CompareExchange(ref _instance, new CachedMemory(), null);
                }
                return _instance;
            }
        }

        static CachedMemory()
        {
            if (File.Exists(PersistenceFilePath))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(PersistenceFilePath, FileMode.Open))
                    {
                        IFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        CachedMemory.Cache = (ConcurrentDictionary<string, object>)bf.Deserialize(fileStream);
                        fileStream.Close();
                    }
                }
                catch (Exception)
                {
                    CachedMemory.Cache = new ConcurrentDictionary<string, object>();
                }
            }
            else
            {
                CachedMemory.Cache = new ConcurrentDictionary<string, object>();
            }
        }

        private CachedMemory()
        {
            Timer stateTimer = new Timer(new TimerCallback(Persist), null, DueTime, Period);
        }

        #endregion

        public void Add(string key, string value)
        {
            CachedMemory.Cache[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return CachedMemory.Cache.ContainsKey(key);
        }

        public object this[string key]
        {
            get { return Cache[key]; }
            set { Cache[key] = value; }
        }
        
        private void Persist(Object stateInfo)
        {
            using (FileStream fileStream = new FileStream(PersistenceFilePath, FileMode.Create))
            {
                IFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(fileStream, CachedMemory.Cache);
                fileStream.Close();
            }
        }

    }
}
