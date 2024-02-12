using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class SenseAppliance : BaseAppliance
{
    [JsonPropertyName("installation_date")]
    public DateTime InstallationDate { get; init; }

    [JsonPropertyName("serial_number")]
    public string SerialNumber { get; init; }

    [JsonPropertyName("version")]
    public string Version { get; init; }

    [JsonPropertyName("timezone")]
    public long Timezone { get; init; }

    [JsonPropertyName("config")]
    public ConfigInfo Config { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; }

    [JsonPropertyName("registration_complete")]
    public bool RegistrationComplete { get; init; }

    public class ConfigInfo
    {
        [JsonPropertyName("thresholds")]
        public Threshold[] Thresholds { get; init; }
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
}