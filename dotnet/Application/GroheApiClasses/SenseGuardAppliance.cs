using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class SenseGuardAppliance : BaseAppliance
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

    [JsonPropertyName("calculate_average_since")]
    public DateTime CalculateAverageSince { get; init; }

    [JsonPropertyName("pressure_notification")]
    public bool PressureNotification { get; init; }

    [JsonPropertyName("snooze_status")]
    public string SnoozeStatus { get; init; }

    [JsonPropertyName("snoozed_from")]
    public DateTime? SnoozedFrom { get; init; }

    [JsonPropertyName("snoozed_until")]
    public DateTime? SnoozedUntil { get; init; }

    public class ConfigInfo
    {
        [JsonPropertyName("thresholds")]
        public Threshold[] Thresholds { get; init; }

        [JsonPropertyName("measurement_period")]
        public long MeasurementPeriod { get; init; }

        [JsonPropertyName("measurement_transmission_longervall")]
        public long MeasurementTransmissionlongervall { get; init; }

        [JsonPropertyName("measurement_transmission_longervall_offset")]
        public long MeasurementTransmissionlongervallOffset { get; init; }

        [JsonPropertyName("action_on_major_leakage")]
        public long ActionOnMajorLeakage { get; init; }

        [JsonPropertyName("action_on_minor_leakage")]
        public long ActionOnMinorLeakage { get; init; }

        [JsonPropertyName("action_on_micro_leakage")]
        public long ActionOnMicroLeakage { get; init; }

        [JsonPropertyName("monitor_frost_alert")]
        public bool MonitorFrostAlert { get; init; }

        [JsonPropertyName("monitor_lower_flow_limit")]
        public bool MonitorLowerFlowLimit { get; init; }

        [JsonPropertyName("monitor_upper_flow_limit")]
        public bool MonitorUpperFlowLimit { get; init; }

        [JsonPropertyName("monitor_lower_pressure_limit")]
        public bool MonitorLowerPressureLimit { get; init; }

        [JsonPropertyName("monitor_upper_pressure_limit")]
        public bool MonitorUpperPressureLimit { get; init; }

        [JsonPropertyName("monitor_lower_temperature_limit")]
        public bool MonitorLowerTemperatureLimit { get; init; }

        [JsonPropertyName("monitor_upper_temperature_limit")]
        public bool MonitorUpperTemperatureLimit { get; init; }

        [JsonPropertyName("monitor_major_leakage")]
        public bool MonitorMajorLeakage { get; init; }

        [JsonPropertyName("monitor_minor_leakage")]
        public bool MonitorMinorLeakage { get; init; }

        [JsonPropertyName("monitor_micro_leakage")]
        public bool MonitorMicroLeakage { get; init; }

        [JsonPropertyName("monitor_system_error")]
        public bool MonitorSystemError { get; init; }

        [JsonPropertyName("monitor_btw_0_1_and_0_8_leakage")]
        public bool MonitorBetweenZeroPolongOneAndZeroPolongEightLeakage { get; init; }

        [JsonPropertyName("monitor_withdrawel_amount_limit_breach")]
        public bool MonitorWithdrawalAmountLimitBreach { get; init; }

        [JsonPropertyName("detection_longerval")]
        public long Detectionlongerval { get; init; }

        [JsonPropertyName("impulse_ignore")]
        public long ImpulseIgnore { get; init; }

        [JsonPropertyName("time_ignore")]
        public long TimeIgnore { get; init; }

        [JsonPropertyName("pressure_tolerance_band")]
        public long PressureToleranceBand { get; init; }

        [JsonPropertyName("pressure_drop")]
        public long PressureDrop { get; init; }

        [JsonPropertyName("detection_time")]
        public long DetectionTime { get; init; }

        [JsonPropertyName("action_on_btw_0_1_and_0_8_leakage")]
        public long ActionOnBetweenZeroPolongOneAndZeroPolongEightLeakage { get; init; }

        [JsonPropertyName("action_on_withdrawel_amount_limit_breach")]
        public long ActionOnWithdrawalAmountLimitBreach { get; init; }

        [JsonPropertyName("withdrawel_amount_limit")]
        public long WithdrawalAmountLimit { get; init; }

        [JsonPropertyName("sprinkler_mode_start_time")]
        public long SprinklerModeStartTime { get; init; }

        [JsonPropertyName("sprinkler_mode_stop_time")]
        public long SprinklerModeStopTime { get; init; }

        [JsonPropertyName("sprinkler_mode_active_monday")]
        public bool SprinklerModeActiveMonday { get; init; }

        [JsonPropertyName("sprinkler_mode_active_tuesday")]
        public bool SprinklerModeActiveTuesday { get; init; }

        [JsonPropertyName("sprinkler_mode_active_wednesday")]
        public bool SprinklerModeActiveWednesday { get; init; }

        [JsonPropertyName("sprinkler_mode_active_thursday")]
        public bool SprinklerModeActiveThursday { get; init; }

        [JsonPropertyName("sprinkler_mode_active_friday")]
        public bool SprinklerModeActiveFriday { get; init; }

        [JsonPropertyName("sprinkler_mode_active_saturday")]
        public bool SprinklerModeActiveSaturday { get; init; }

        [JsonPropertyName("sprinkler_mode_active_sunday")]
        public bool SprinklerModeActiveSunday { get; init; }
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
