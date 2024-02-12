using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Application.GroheApiClasses;

public class SenseDetails
{
    [JsonPropertyName("appliance_id")]
    public string ApplianceId { get; init; }

    [JsonPropertyName("installation_date")]
    public DateTime InstallationDate { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("serial_number")]
    public string SerialNumber { get; init; }

    [JsonPropertyName("type")]
    public long Type { get; init; }

    [JsonPropertyName("version")]
    public string Version { get; init; }

    [JsonPropertyName("tdt")]
    public DateTime LastSeen { get; init; }

    [JsonPropertyName("timezone")]
    public long Timezone { get; init; }

    [JsonPropertyName("config")]
    public ConfigInfo Config { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; }

    [JsonPropertyName("registration_complete")]
    public bool RegistrationComplete { get; init; }

    [JsonPropertyName("status")]
    public List<StatusInfo> Status { get; init; }

    [JsonPropertyName("data_latest")]
    public DataLatestInfo DataLatest { get; init; }

    [JsonPropertyName("notifications")]
    public List<object> Notifications { get; init; } // unknown type :/

    [JsonPropertyName("snooze_status")]
    public string SnoozeStatus { get; init; }
    
    public class ConfigInfo
    {
        [JsonPropertyName("thresholds")]
        public List<Threshold> Thresholds { get; init; }
    }

    public class Threshold
    {
        [JsonPropertyName("quantity")]
        public string Quantity { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; }

        [JsonPropertyName("value")]
        public long Value { get; init; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }
    }

    public class StatusInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; init; }

        [JsonPropertyName("value")]
        public long Value { get; init; }
    }

    public class DataLatestInfo
    {
        [JsonPropertyName("measurement")]
        public MeasurementInfo Measurement { get; init; }
    }

    public class MeasurementInfo
    {
        [JsonPropertyName("battery")]
        public long Battery { get; init; }

        [JsonPropertyName("humidity")]
        public long Humidity { get; init; }

        [JsonPropertyName("temperature")]
        public decimal Temperature { get; init; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; init; }
    }
}
