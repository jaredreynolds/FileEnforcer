using Microsoft.Extensions.Caching.Memory;
using FileEnforcer.Extensions;

namespace FileEnforcer.Services;

public class DebouncerService<TItem, TContext>
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly Func<TItem, object> _keySelector;
    private readonly PostEvictionDelegate _postEvictionDelegate;

    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(5);
    public ILogger? Logger { get; set; }

    public DebouncerService(
        Func<TItem, object> keySelector,
        Action<object, TItem, TContext> postDebounceAction,
        ILogger? logger = null)
    {
        _keySelector = keySelector;
        _postEvictionDelegate = (key, value, reason, state) => postDebounceAction(key, (TItem)value!, (TContext)state!);
        Logger = logger;
    }

    public void Debounce(TItem item, TContext context)
    {
        Logger?.LogTrace("{timestamp} In debounce", DateTime.Now.TimeOfDay);

        _cache.GetOrCreate(_keySelector(item), cacheEntry =>
        {
            Logger?.LogTrace("{timestamp} Creating cache entry", DateTime.Now.TimeOfDay);

            cacheEntry
                .AddExpirationToken(Delay)
                .SetAbsoluteExpiration(Delay)
                .RegisterPostEvictionCallback(_postEvictionDelegate, context);

            return item;
        });
    }
}
