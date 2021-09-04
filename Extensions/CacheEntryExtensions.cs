using System;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace FileEnforcer.Extensions
{
    public static class CacheEntryExtensions
    {
        public static ICacheEntry AddExpirationToken(this ICacheEntry cacheEntry, TimeSpan delay)
        {
            var cancellationTokenSource = new CancellationTokenSource(delay);
            var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
            return cacheEntry?.AddExpirationToken(cancellationChangeToken);
        }
    }
}
