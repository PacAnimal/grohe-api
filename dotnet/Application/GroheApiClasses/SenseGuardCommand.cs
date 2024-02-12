using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class SenseGuardCommand
{
    [JsonPropertyName("appliance_id")]
    public string ApplianceId { get; init; }

    [JsonPropertyName("type")]
    public long Type { get; init; }

    [JsonPropertyName("command")]
    public CommandInfo Command { get; init; }

    [JsonPropertyName("commandb64")]
    public string CommandBase64 { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
    
    public class CommandInfo
    {
        [JsonPropertyName("temp_user_unlock_on")]
        public bool TempUserUnlockOn { get; init; }

        [JsonPropertyName("reason_for_change")]
        public long ReasonForChange { get; init; }

        [JsonPropertyName("pressure_measurement_running")]
        public bool PressureMeasurementRunning { get; init; }

        [JsonPropertyName("buzzer_on")]
        public bool BuzzerOn { get; init; }

        [JsonPropertyName("buzzer_sound_profile")]
        public long BuzzerSoundProfile { get; init; }

        [JsonPropertyName("valve_open")]
        public bool ValveOpen { get; set; }

        [JsonPropertyName("measure_now")]
        public bool MeasureNow { get; init; }
    }
}