using BW.Libs.Config.Abstractions;
using BW.Libs.Config.Tests.TestVessels;

namespace BW.Libs.Config.Tests.TestServices;

public sealed class TestConfigService
{
    private readonly IVesselFactory _vesselFactory;

    public TestConfigService(IVesselFactory vesselFactory)
    {
        _vesselFactory = vesselFactory ?? throw new ArgumentNullException(nameof(vesselFactory));
    }

    public async Task<int> GetMaxRetriesAsync()
    {
        var vessel = await _vesselFactory.GetVesselAsync<TestConfigVessel>();
        return vessel.MaxRetries;
    }

    public async Task<string> GetServiceNameAsync()
    {
        var vessel = await _vesselFactory.GetVesselAsync<TestConfigVessel>();
        return vessel.ServiceName;
    }
}
