using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Demo.Common.Utils.Cache
{
    public static class DistributedCacheExtensions
    {
        public static Task SetAsync<T>(this IDistributedCache _cache, string id, T entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            return _cache.SetStringAsync(id, JsonSerializer.Serialize(entry), options, cancellationToken);
        }
        
        public static async Task<bool> ExistsAsync(this IDistributedCache _cache, string id, CancellationToken cancellationToken = default)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            return !string.IsNullOrEmpty(stored);
        }
        
        public static async Task<T> GetAsync<T>(this IDistributedCache _cache, string id, CancellationToken cancellationToken = default)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            if (!string.IsNullOrEmpty(stored))
            {
                return JsonSerializer.Deserialize<T>(stored);
            }

            return default;
        }
        
        public static async Task RemoveAsync(this IDistributedCache _cache, string id, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(id, cancellationToken);
        }
    }
}
