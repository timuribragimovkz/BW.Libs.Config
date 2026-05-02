using Amazon.DynamoDBv2;
using Amazon.Runtime;
using BW.Libs.Config.DiExtensions;
using BW.Libs.Config.Tests.Framework.AwsCredentials;
using Microsoft.Extensions.DependencyInjection;

namespace BW.Libs.Config.Tests.Framework.DiExtensions;

public static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddTestAwsServices(
        this IServiceCollection services,
        TestAwsSessionCredentials credentials,
        string region = "eu-central-1")
    {
        var awsCredentials = new SessionAWSCredentials(
            credentials.AwsAccessKeyId,
            credentials.AwsSecretAccessKey,
            credentials.AwsSessionToken);

        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(awsCredentials, Amazon.RegionEndpoint.GetBySystemName(region)));

        return services;
    }

    public static IServiceCollection AddTestConfigEngine(
        this IServiceCollection services,
        string dynamoDbTableName,
        params System.Reflection.Assembly[] assemblies)
    {
        return services.AddConfigEngine(dynamoDbTableName, assemblies);
    }
}
