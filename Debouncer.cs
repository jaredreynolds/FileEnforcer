using System;
using Microsoft.Extensions.Caching.Memory;
using FileEnforcer.Extensions;

namespace FileEnforcer
{
    public class Debouncer<TItem, TContext>
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly Func<TItem, object> _keySelector;
        private readonly PostEvictionDelegate _postEvictionDelegate;

        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(5);

        public Debouncer(
            Func<TItem, object> keySelector,
            Action<object, TItem, TContext> postDebounceAction)
        {
            _keySelector = keySelector;
            _postEvictionDelegate = (key, value, reason, state) => postDebounceAction(key, (TItem)value, (TContext)state);
        }

        public void Debounce(TItem item, TContext context)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay} In Debounce");

            _cache.GetOrCreate(_keySelector(item), cacheEntry =>
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay} Creating cache entry");

                cacheEntry
                    .AddExpirationToken(Delay)
                    .SetAbsoluteExpiration(Delay)
                    .RegisterPostEvictionCallback(_postEvictionDelegate, context);

                return item;
            });
        }
    }
}
