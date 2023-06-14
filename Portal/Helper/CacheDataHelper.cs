namespace Demo.Portal.Helper
{
    public class CacheDataHelper
    {
        private Dictionary<string, object> defaultCacheItem = new Dictionary<string, object>();

        /// <summary>
        /// Configuration values dictionary
        /// </summary>
        public Dictionary<string, object> ConfigDictionary
        {
            get { return defaultCacheItem; }
        }

        private static CacheDataHelper instance;
        public static CacheDataHelper Instance
        {
            get
            {
                if (instance == null)
                    instance = new CacheDataHelper();
                return instance;
            }
        }

        // private constructor
        private CacheDataHelper() { }

        public void SetCache(string key, object value)
        {
            if (!defaultCacheItem.ContainsKey(key))
                defaultCacheItem.Add(key, value);
        }

        public object GetCache(string key)
        {
            if (defaultCacheItem.ContainsKey(key))
                return defaultCacheItem[key];
            return null;
        }

        public object GetAllCache()
        {
            return defaultCacheItem;
        }

        public void ClearCache(string key)
        {
            if (defaultCacheItem.ContainsKey(key))
                defaultCacheItem.Remove(key);
        }
    }
}
