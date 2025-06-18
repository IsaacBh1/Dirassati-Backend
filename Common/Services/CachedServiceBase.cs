using System;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Caching.Memory;

namespace Dirassati_Backend.Common.Services;

public class CachedServiceBase(IMemoryCache cache, ILogger<CachedServiceBase> logger)
{
    public async Task<Result<TResult, string>> GetOrSetCacheAsync<TResult>(string cacheKey, MemoryCacheEntryOptions cacheOptions, Func<Task<Result<TResult, string>>> factory)
    {
        var result = new Result<TResult, string>();
        if (cache.TryGetValue(cacheKey, out TResult? entry))
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return result.Success(entry!);
        }

        logger.LogDebug("Cache miss for key: {CacheKey}. Executing factory method.", cacheKey);
        var factoryResult = await factory();

        if (factoryResult.IsSuccess)
        {
            cacheOptions.Size ??= 1;
            cache.Set(cacheKey, factoryResult.Value, cacheOptions);
            logger.LogDebug("Cache set for key: {CacheKey}", cacheKey);
        }
        else
        {
            logger.LogWarning("Factory method failed for cache key: {CacheKey}. Error: {Error}", cacheKey, factoryResult.Errors);
        }

        return factoryResult;
    }

    protected void InvalidateCache(string cacheKey)
    {
        logger.LogDebug("Invalidating cache for key: {CacheKey}", cacheKey);
        cache.Remove(cacheKey);
    }
}
