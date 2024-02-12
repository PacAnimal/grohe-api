using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class Location
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("type")]
    public long Type { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; init; }

    [JsonPropertyName("water_cost")]
    public decimal WaterCost { get; init; }

    [JsonPropertyName("energy_cost")]
    public decimal EnergyCost { get; init; }

    [JsonPropertyName("heating_type")]
    public long HeatingType { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; }

    [JsonPropertyName("default_water_cost")]
    public decimal DefaultWaterCost { get; init; }

    [JsonPropertyName("default_energy_cost")]
    public decimal DefaultEnergyCost { get; init; }

    [JsonPropertyName("default_heating_type")]
    public long DefaultHeatingType { get; init; }

    [JsonPropertyName("emergency_shutdown_enable")]
    public bool EmergencyShutdownEnable { get; init; }

    [JsonPropertyName("address")]
    public AddressInfo Address { get; init; }
}

public class AddressInfo
{
    [JsonPropertyName("street")]
    public string Street { get; init; }

    [JsonPropertyName("city")]
    public string City { get; init; }

    [JsonPropertyName("zipcode")]
    public string Zipcode { get; init; }

    [JsonPropertyName("housenumber")]
    public string HouseNumber { get; init; }

    [JsonPropertyName("country")]
    public string Country { get; init; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; init; }

    [JsonPropertyName("additionalInfo")]
    public string AdditionalInfo { get; init; }

    [JsonPropertyName("state")]
    public string State { get; init; }
}