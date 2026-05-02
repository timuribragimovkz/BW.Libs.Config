using BW.Libs.Config.Abstractions;
using BW.Libs.Config.Components;
using BW.Libs.Config.Contracts;
using BW.Libs.Config.Tests.Framework.AwsCredentials;
using BW.Libs.Config.Tests.Framework.DiExtensions;
using BW.Libs.Config.Tests.TestServices;
using BW.Libs.Config.Tests.TestVessels;
using Microsoft.Extensions.DependencyInjection;

namespace BW.Libs.Config.Tests;

public class ConfigEngineIntegrationTests
{
    private const string DynamoDbTableName = "Bruceware-Grooming-API-Config";
    private const string AwsRegion = "eu-central-1";
    private const string AwsCredentialsFilePath = ".aws-credentials-test";

    private ServiceProvider _serviceProvider = null!;
    private TestConfigService _testConfigService = null!;
    private IConfigSource _configSource = null!;
    private ConfigRepository _configRepository = null!;

    [SetUp]
    public void Setup()
    {
        var awsCredentials = AwsCredentialsParsingUtilities.GetAwsCredentialsFromFile(AwsCredentialsFilePath);

        var services = new ServiceCollection();

        services.AddTestAwsServices(awsCredentials, AwsRegion);
        services.AddTestConfigEngine(DynamoDbTableName, typeof(TestConfigVessel).Assembly);
        services.AddSingleton<TestConfigService>();

        _serviceProvider = services.BuildServiceProvider();

        _testConfigService = _serviceProvider.GetRequiredService<TestConfigService>();
        _configSource = _serviceProvider.GetRequiredService<IConfigSource>();
        _configRepository = _serviceProvider.GetRequiredService<ConfigRepository>();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [Test]
    public async Task GetVesselAsync_WhenConfigChanges_ReturnsUpdatedValue()
    {
        // Arrange - Seed initial config
        var initialEnvelope = new ConfigEnvelope
        {
            VesselData = new
            {
                maxRetries = 5,
                serviceName = "InitialService"
            }
        };

        await _configRepository.SaveConfigEnvelopeAsync(nameof(TestConfigVessel), initialEnvelope);

        // Act - Get initial value
        var initialMaxRetries = await _testConfigService.GetMaxRetriesAsync();

        // Assert initial
        Assert.That(initialMaxRetries, Is.EqualTo(5), "Initial MaxRetries should be 5");

        // Arrange - Update config
        var updatedEnvelope = new ConfigEnvelope
        {
            VesselData = new
            {
                maxRetries = 10,
                serviceName = "UpdatedService"
            }
        };

        await _configRepository.SaveConfigEnvelopeAsync(nameof(TestConfigVessel), updatedEnvelope);

        // Act - Get updated value
        var updatedMaxRetries = await _testConfigService.GetMaxRetriesAsync();

        // Assert updated
        Assert.That(updatedMaxRetries, Is.EqualTo(10), "Updated MaxRetries should be 10");
    }
}