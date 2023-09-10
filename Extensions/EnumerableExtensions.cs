namespace FileEnforcer.Extensions;

public static class EnumerableExtensions
{
    public static void AddRange<T>(this ICollection<T> source, params T[] items)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        items.ForEach(item => source.Add(item));
    }

    public static void AddRange<TKey, TValue>(
        this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        keyValuePairs.ForEach(keyValuePair => source.Add(keyValuePair));
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        foreach (var item in source)
        {
            action(item);
        }
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => source is null || !source.Any();
}
