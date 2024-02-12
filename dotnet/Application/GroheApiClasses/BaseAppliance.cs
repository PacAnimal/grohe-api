using System.Text.Json;
using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Application.GroheApiClasses;

public class BaseAppliance
{
    // common properties
    [JsonPropertyName("appliance_id")]
    public string Id { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("tdt")]
    public DateTime? LastSeen { get; init; }
    
    // related objects
    [JsonIgnore]
    public Room Room { get; set; }

    [JsonIgnore]
    public Location Location { get; set; }

    [JsonIgnore]
    public Dictionary<string, long> Status { get; private set; }
    
    // manually added by the BaseApplianceConverter
    [JsonIgnore]
    public ApplianceType Type { get; set; }
    [JsonIgnore]
    public long TypeValue { get; set; }
    [JsonIgnore]
    public string Json { get; set; }
    
    // the serializer for BaseAppliance
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new BaseApplianceConverter<BaseAppliance>() }
    };

    // converting status to a dictionary
    public void SetStatus(List<ApplianceStatus> status)
    {
        Status = status.ToDictionary(x => x.Type, x => x.Value); 
    }
}

public class BaseApplianceConverter<T> : JsonConverter<T> where T : BaseAppliance, new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var jsonObject = document.RootElement;
        var json = jsonObject.GetRawText();

        var typeValue = jsonObject.GetProperty("type").GetInt32();
        var type = Enum.IsDefined(typeof(ApplianceType), typeValue) ? (ApplianceType)typeValue : ApplianceType.Unknown;
        
        var baseAppliance = JsonSerializer.Deserialize<T>(jsonObject.GetRawText());
        baseAppliance.TypeValue = typeValue;
        baseAppliance.Type = type;
        baseAppliance.Json = json;

        return baseAppliance;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

public enum ApplianceType
{
    Unknown = 0,
    Sense = 101,
    SenseGuard = 103,
}