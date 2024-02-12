using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class SenseGuardSnoozeCommand
{
    [JsonPropertyName("snooze_duration")]
    public long SnoozeMinutes { get; init; }
}