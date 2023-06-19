namespace Demo.Portal.Helper;

public class CacheDataHelper
{
    private static CacheDataHelper instance;

    // private constructor
    private CacheDataHelper()
    {
    }

    /// <summary>
    ///     Configuration values dictionary
    /// </summary>
    public Dictionary<string, object> ConfigDictionary { get; } = new();

    public static CacheDataHelper Instance
    {
        get
        {
            if (instance == null)
                instance = new CacheDataHelper();
            return instance;
        }
    }

    public void SetCache(string key, object value)
    {
        if (!ConfigDictionary.ContainsKey(key))
            ConfigDictionary.Add(key, value);
    }

    public object GetCache(string key)
    {
        if (ConfigDictionary.ContainsKey(key))
            return ConfigDictionary[key];
        return null;
    }

    public object GetAllCache()
    {
        return ConfigDictionary;
    }

    public void ClearCache(string key)
    {
        if (ConfigDictionary.ContainsKey(key))
            ConfigDictionary.Remove(key);
    }
}