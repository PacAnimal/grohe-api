using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class AggregateData
{
    [JsonPropertyName("appliance_id")]
    public string ApplianceId { get; init; }

    [JsonPropertyName("type")]
    public long TypeValue { get; init; }

    [JsonPropertyName("data")]
    public Payload Data { get; init; }
    
    public class Payload
    {
        [JsonPropertyName("group_by")]
        public string Aggregation { get; init; }

        [JsonPropertyName("measurement")]
        public List<Measurement> Measurements { get; init; }

        [JsonPropertyName("withdrawals")]
        public List<Withdrawal> Withdrawals { get; init; }
    }

    public class Measurement
    {
        [JsonPropertyName("date")]
        public string When { get; init; }

        [JsonPropertyName("flowrate")]
        public decimal? FlowRate { get; init; }

        [JsonPropertyName("pressure")]
        public decimal? Pressure { get; init; }

        [JsonPropertyName("temperature_guard")]
        public decimal? TemperatureGuard { get; init; }

        [JsonPropertyName("temperature")]
        public decimal? Temperature { get; init; }

        [JsonPropertyName("humidity")]
        public long? Humidity { get; init; }
    }

    public class Withdrawal
    {
        [JsonPropertyName("date")]
        public string When { get; init; }

        [JsonPropertyName("waterconsumption")]
        public decimal? WaterConsumption { get; init; }

        [JsonPropertyName("hotwater_share")]
        public decimal? HotWaterShare { get; init; }

        [JsonPropertyName("water_cost")]
        public decimal? WaterCost { get; init; }

        [JsonPropertyName("energy_cost")]
        public decimal? EnergyCost { get; init; }
    }
}

public enum Aggregation
{
    Unknown,
    Hour,
    Day,
    Week,
    Month,
    Year,
}