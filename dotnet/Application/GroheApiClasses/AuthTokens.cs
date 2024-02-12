using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class AuthTokens
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }

    [JsonPropertyName("refresh_expires_in")]
    public long RefreshExpiresIn { get; init; }
    
    [JsonIgnore]
    private DateTime CreatedAt { get; } = DateTime.Now;
    
    [JsonIgnore]
    // ReSharper disable once PossibleLossOfFraction
    public DateTime RefreshAt => CreatedAt.AddSeconds(ExpiresIn / 2);
}