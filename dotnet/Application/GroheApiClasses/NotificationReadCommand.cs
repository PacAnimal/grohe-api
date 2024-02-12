using System.Text.Json.Serialization;
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class NotificationReadCommand(string applianceId, long category, long type, string notificationId, DateTime timestamp)
{
    // all these uninitialized properties are sent as 0 by the app - so let's just do the same
    [JsonPropertyName("_id")]
    public long Id { get; }

    [JsonPropertyName("appliance_id")]
    public string ApplianceId { get; } = applianceId;

    [JsonPropertyName("category")]
    public long Category { get; } = category;

    [JsonPropertyName("eventReactionId")]
    public long EventReactionId { get; }

    [JsonPropertyName("id")]
    public string NotificationId { get; } = notificationId;

    [JsonPropertyName("is_read")]
    public bool IsRead { get; init; } = true;

    [JsonPropertyName("location_id")]
    public long LocationId { get; }

    [JsonPropertyName("rawCategory")]
    public long RawCategory { get; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; } = timestamp;

    [JsonPropertyName("type")]
    public long Type { get; } = type;
}