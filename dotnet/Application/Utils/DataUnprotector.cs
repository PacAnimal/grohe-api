using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
// ReSharper disable UnusedMethodReturnValue.Global

namespace Application.Utils;

public static class DataUnprotector
{
    public static IDataProtectionBuilder PersistKeysToNowhere(this IDataProtectionBuilder builder)
    {
        builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(_ => new ConfigureOptions<KeyManagementOptions>(options =>
        {
            options.XmlRepository = new MemoryXmlRepository();
            options.XmlEncryptor = new NullXmlEncryptor();
        }));

        return builder;
    }
}

public class MemoryXmlRepository : IXmlRepository
{
    private readonly Dictionary<string, XElement> _elements = new Dictionary<string, XElement>();
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return _elements.Values.ToList();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        _elements[friendlyName] = element;
    }
}