namespace Application.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
public class JsonTypeNameAttribute(string schemaId) : Attribute
{
    public string SchemaId { get; } = schemaId;
}