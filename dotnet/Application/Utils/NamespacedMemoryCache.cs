using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
// ReSharper disable UnusedMember.Global

namespace Application.Utils;

// ReSharper disable MemberCanBePrivate.Global

public class NamespacedMemoryCache(IMemoryCache cache, string @namespace) : IMemoryCache
{
    public string Namespace { get; } = $"{AssemblyName}|{@namespace}";

    private static readonly string AssemblyName;

    static NamespacedMemoryCache()
    {
        AssemblyName = Assembly.GetEntryAssembly()?.GetName().Name
                       ?? throw new Exception($"{nameof(NamespacedMemoryCache)} can not be used by unmanaged executables");
    }

    private string Key(string key) => $"{Namespace}|{key}";
    private string Key(object key) => key is string s ? Key(s) : Key(JsonSerializer.Serialize(key));

    public ICacheEntry CreateEntry(object key) => cache.CreateEntry(Key(key));

    public void Remove(object key) => cache.Remove(Key(key));

    public bool TryGetValue(object key, out object value) => cache.TryGetValue(Key(key), out value);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        cache.Dispose();
    }
}

public static class MemoryCacheExtensions
{
    public static TValue FluentSet<TValue>(this IMemoryCache cache, object key, TValue value, TimeSpan lifeSpan)
    {
        cache.Set(key, value, lifeSpan);
        return value;
    }

    public static async Task<TValue> GetOrRefreshAsync<TValue>(this IMemoryCache cache, object key, Func<Task<TValue>> valueGenerator, TimeSpan lifeSpan)
    {
        if (!cache.TryGetValue<TValue>(key, out var value))
        {
            value = cache.FluentSet(
                key,
                await valueGenerator.Invoke(),
                lifeSpan
            );
        }
        return value;
    }

    public static NamespacedMemoryCache WithNamespace(this IMemoryCache cache, string @namespace) =>
        cache is NamespacedMemoryCache nmc
            ? throw new ArgumentException($"Cache already has a namespace: {nmc.Namespace}")
            : new NamespacedMemoryCache(cache, @namespace);
}