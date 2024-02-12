using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Application.Utils;

[SuppressMessage("Performance", "SYSLIB1045:Convert to \'GeneratedRegexAttribute\'.")]
public static class Extensions
{
    public static string GetString(this IConfiguration config, string key, string defaultValue = null)
    {
        var value = config.GetValue<string>(key);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : !string.IsNullOrWhiteSpace(defaultValue)
                ? defaultValue
                : throw new ArgumentException($"Configuration value for {key} is empty");
    }
    
    public static Uri ParseUri(this string s) => Uri.TryCreate(s, UriKind.Absolute, out var uri) ? uri : throw new ArgumentException( $"Attempting to parse invalid URI: {s}", nameof(s));
    
    public static long ToUnixTime(this DateTime dateTime)
    {
        var dto = new DateTimeOffset(dateTime.ToUniversalTime());
        return dto.ToUnixTimeSeconds();
    }
}