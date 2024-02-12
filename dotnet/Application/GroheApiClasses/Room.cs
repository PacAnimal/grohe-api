using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class Room
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("type")]
    public long Type { get; init; }

    [JsonPropertyName("room_type")]
    public long RoomType { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; }
}