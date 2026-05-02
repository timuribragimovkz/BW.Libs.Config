using System.Reflection;
using Amazon.DynamoDBv2;
using BW.Libs.Config.Abstractions;
using BW.Libs.Config.Components;
using BW.Libs.Config.Sources;
using BW.Libs.Config.Sources.DDB;
using Microsoft.Extensions.DependencyInjection;

namespace BW.Libs.Config.DiExtensions;

/// <summary>
/// Dependency injection extensions for BW.Libs.Config
/// </summary>
public static class ConfigEngineRegistration
{
    /// <summary>
    /// Registers configuration engine with DynamoDB source and scans assemblies for vessels.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="dynamoDbTableName">DynamoDB table name for configuration storage</param>
    /// <param name="assemblies">Assemblies to scan for IVessel implementations</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddConfigEngine(
        this IServiceCollection services,
        string dynamoDbTableName,
        params Assembly[] assemblies)
    {
        if (string.IsNullOrWhiteSpace(dynamoDbTableName))
            throw new ArgumentException("DynamoDB table name cannot be null or empty.", nameof(dynamoDbTableName));

        // Register DynamoDB source
        services.AddSingleton<IConfigSource>(sp =>
        {
            var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
            return new DynamoDbConfigSource(dynamoDb, dynamoDbTableName);
        });

        // Register repository and factory
        services.AddSingleton<ConfigRepository>();
        services.AddSingleton<IVesselFactory, VesselFactory>();

        // Scan and register vessels (pre-registration for eager initialization if needed)
        var vesselTypes = VesselScanner.ScanForVessels(assemblies);
        foreach (var vesselType in vesselTypes)
        {
            // Vessels are managed by factory, but we can register metadata here if needed
            // For now, just validation that scan works
        }

        return services;
    }

    /// <summary>
    /// Registers configuration engine with custom config source and scans assemblies for vessels.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan for IVessel implementations</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddConfigEngine(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Assumes IConfigSource is already registered
        services.AddSingleton<ConfigRepository>();
        services.AddSingleton<IVesselFactory, VesselFactory>();

        // Scan vessels
        var vesselTypes = VesselScanner.ScanForVessels(assemblies);
        foreach (var vesselType in vesselTypes)
        {
            // Pre-registration placeholder
        }

        return services;
    }
}
