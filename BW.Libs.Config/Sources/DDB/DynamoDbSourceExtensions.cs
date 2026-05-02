using Amazon.DynamoDBv2;
using BW.Libs.Config.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BW.Libs.Config.Sources.DDB;

/// <summary>
/// DI extensions for registering DynamoDB configuration source.
/// </summary>
public static class DynamoDbSourceExtensions
{
    /// <summary>
    /// Registers DynamoDB as the configuration source.
    /// Requires IAmazonDynamoDB to be registered separately.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="tableName">DynamoDB table name</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDynamoDbConfigSource(
        this IServiceCollection services,
        string tableName
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

        services.AddSingleton<IConfigSource>(sp =>
            {
                var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
                return new DynamoDbConfigSource(dynamoDb, tableName);
            }
        );

        return services;
    }
}
