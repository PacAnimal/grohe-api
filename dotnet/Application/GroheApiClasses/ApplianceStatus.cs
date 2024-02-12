using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class ApplianceStatus
{
    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonPropertyName("value")]
    public long Value { get; init; }
}
