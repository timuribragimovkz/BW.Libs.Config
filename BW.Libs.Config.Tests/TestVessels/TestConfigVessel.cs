using System.Text.Json;
using BW.Libs.Config.Contracts;

namespace BW.Libs.Config.Tests.TestVessels;

public sealed class TestConfigVessel : IVessel<TestConfigVessel>
{
    public string CurrentConfigHash { get; private set; } = string.Empty;
    public int MaxRetries { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;

    public void FromJson(string vesselDataJson)
    {
        var jsonDoc = JsonDocument.Parse(vesselDataJson);
        var root = jsonDoc.RootElement;

        MaxRetries = root.GetProperty("maxRetries").GetInt32();
        ServiceName = root.GetProperty("serviceName").GetString() ?? string.Empty;
    }

    public string ToJson()
    {
        var data = new
        {
            maxRetries = MaxRetries,
            serviceName = ServiceName
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    public void Validate()
    {
        if (MaxRetries <= 0)
            throw new InvalidOperationException("MaxRetries must be greater than zero.");

        if (string.IsNullOrWhiteSpace(ServiceName))
            throw new InvalidOperationException("ServiceName cannot be empty.");
    }
}
