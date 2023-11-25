using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Constants;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Common.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async ValueTask<(bool IsExist, T Value)> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            if (bytes == null)
            {
                return (false, default);
            }

            return (true, JsonSerializer.Deserialize<T>(bytes, AppJsonOptions.Default));
        }
        
        public static async ValueTask SetAsync<T>(this IDistributedCache cache, string key, T value, TimeSpan absoluteExpirationRelativeToNow = default, CancellationToken cancellationToken = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, AppJsonOptions.Default);

            if (absoluteExpirationRelativeToNow == default)
            {
                await cache.SetAsync(key, bytes, cancellationToken);
            }
            else
            {
                await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions(){ AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow}, cancellationToken);
            }
        }

        public static async ValueTask<T> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> valueFactory,
            TimeSpan absoluteExpirationRelativeToNow = default, CancellationToken cancellationToken = default)
        {
            var bytes = await cache.GetAsync(key, cancellationToken);

            if (bytes != null)
            {
                return JsonSerializer.Deserialize<T>(bytes, AppJsonOptions.Default);
            }

            var value = await valueFactory();

            bytes = JsonSerializer.SerializeToUtf8Bytes(value, AppJsonOptions.Default);

            if (absoluteExpirationRelativeToNow == default)
            {
                await cache.SetAsync(key, bytes, cancellationToken);
            }
            else
            {
                await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions(){ AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow}, cancellationToken);
            }

            return value;
        }

        public static async ValueTask<T> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<ValueTask<T>> valueFactory,
            TimeSpan absoluteExpirationRelativeToNow = default, CancellationToken cancellationToken = default)
        {
            var bytes = await cache.GetAsync(key, cancellationToken);

            if (bytes != null)
            {
                return JsonSerializer.Deserialize<T>(bytes, AppJsonOptions.Default);
            }

            var value = await valueFactory();

            bytes = JsonSerializer.SerializeToUtf8Bytes(value, AppJsonOptions.Default);

            if (absoluteExpirationRelativeToNow == default)
            {
                await cache.SetAsync(key, bytes, cancellationToken);
            }
            else
            {
                await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions(){ AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow}, cancellationToken);
            }

            return value;
        }
    }
}
