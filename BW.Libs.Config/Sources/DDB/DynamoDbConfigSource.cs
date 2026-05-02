using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BW.Libs.Config.Abstractions;
using BW.Libs.Config.Exceptions;

namespace BW.Libs.Config.Sources.DDB;

/// <summary>
/// DynamoDB implementation of IConfigSource.
/// Expects table with partition key 'ConfigClassName' (string) and attributes 'JsonData' (string).
/// </summary>
internal sealed class DynamoDbConfigSource : IConfigSource
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DynamoDbConfigSource(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb ?? throw new ArgumentNullException(nameof(dynamoDb));
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    public async Task<string> GetJsonAsync(string configName)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "ConfigClassName", new AttributeValue { S = configName } }
            }
        };

        try
        {
            var response = await _dynamoDb.GetItemAsync(request);

            if (!response.IsItemSet || !response.Item.ContainsKey("JsonData"))
                throw new ConfigNotFoundException(configName);

            return response.Item["JsonData"].S;
        }
        catch (ResourceNotFoundException ex)
        {
            throw new ConfigNotFoundException(configName, ex);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"DynamoDB error while retrieving config '{configName}': {ex.Message}", ex);
        }
    }

    public async Task UpdateJsonAsync(string configName, string json)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "ConfigClassName", new AttributeValue { S = configName } },
                { "JsonData", new AttributeValue { S = json } }
            }
        };

        try
        {
            await _dynamoDb.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"DynamoDB error while updating config '{configName}': {ex.Message}", ex);
        }
    }
}
