using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class NotificationContainer
{
    [JsonPropertyName("continuation_token")]
    public string ContinuationToken { get; init; }

    [JsonPropertyName("remaining_notifications")]
    public long? RemainingNotifications { get; init; }

    [JsonPropertyName("notifications")]
    public List<Notification> Notifications { get; init; }
}
public class Notification
{
    [JsonPropertyName("notification_id")]
    public string Id { get; init; }
    
    [JsonPropertyName("appliance_id")]
    public string ApplianceId { get; init; }

    [JsonPropertyName("location_id")]
    public long LocationId { get; init; }

    [JsonPropertyName("category")]
    public long Category { get; init; }

    [JsonPropertyName("is_read")]
    public bool IsRead { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("notification_type")]
    public long NotificationType { get; init; }

    [JsonIgnore]
    public string ApplianceName { get; set; }

    [JsonIgnore]
    public string Message { get; set; }

    [JsonIgnore]
    public string LocationName { get; set; }

    public ApplianceType ApplianceType { get; set; }
}

