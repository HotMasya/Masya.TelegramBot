using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetRecordAsync<T>(
            this IDistributedCache cache,
            string recordId,
            T item,
            TimeSpan? absoluteExpirationTime = null,
            TimeSpan? slidingExpirationTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationTime ?? TimeSpan.FromSeconds(60),
                SlidingExpiration = slidingExpirationTime
            };
            var jsonData = JsonSerializer.Serialize(item);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T> GetRecordAsync<T>(
            this IDistributedCache cache,
            string recordId
        )
        {
            var jsondata = await cache.GetStringAsync(recordId);

            if (jsondata is null)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(jsondata);
        }
    }
}