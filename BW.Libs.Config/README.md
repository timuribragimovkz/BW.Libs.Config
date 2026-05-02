# BW.Libs.Config

A DDD-focused configuration engine with runtime change detection, hash-based cache invalidation, and pluggable sources.

## Features

- **Domain-Driven Design**: Configuration as rich domain objects (Vessels) with validation and behavior
- **Runtime Updates**: Automatic cache invalidation via hash-based change detection
- **Pluggable Sources**: DynamoDB, Redis, file system, AWS Parameter Store, or custom implementations
- **Thread-Safe**: Built-in concurrency control with `SemaphoreSlim` per vessel type
- **Type-Safe**: Generic `IVessel<T>` interface with self-referencing constraint
- **Zero Overhead**: Cached vessels avoid deserialization when hash unchanged

## Installation

```bash
dotnet add package BW.Libs.Config
```

## Quick Start

### 1. Define a Vessel

```csharp
using System.Text.Json;
using BW.Libs.Config.Contracts;

public sealed class DatabaseConfigVessel : IVessel<DatabaseConfigVessel>
{
    public string CurrentConfigHash { get; private set; } = string.Empty;
    public string ConnectionString { get; private set; } = string.Empty;
    public int MaxRetries { get; private set; }
    public int TimeoutSeconds { get; private set; }

    public void FromJson(string vesselDataJson)
    {
        var json = JsonDocument.Parse(vesselDataJson).RootElement;
        ConnectionString = json.GetProperty("connectionString").GetString() ?? string.Empty;
        MaxRetries = json.GetProperty("maxRetries").GetInt32();
        TimeoutSeconds = json.GetProperty("timeoutSeconds").GetInt32();
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            connectionString = ConnectionString,
            maxRetries = MaxRetries,
            timeoutSeconds = TimeoutSeconds
        });
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("ConnectionString is required.");
        if (MaxRetries <= 0)
            throw new InvalidOperationException("MaxRetries must be positive.");
        if (TimeoutSeconds <= 0)
            throw new InvalidOperationException("TimeoutSeconds must be positive.");
    }

    // Domain methods
    public bool ShouldRetry(int attemptNumber) => attemptNumber < MaxRetries;
}
```

### 2. Register Services (DynamoDB Source)

```csharp
using BW.Libs.Config.DiExtensions;
using BW.Libs.Config.Sources.DiExtensions;

// Startup.cs or Program.cs
services.AddAWSService<IAmazonDynamoDB>();
services.AddDynamoDbConfigSource("my-config-table");
services.AddConfigEngine(typeof(DatabaseConfigVessel).Assembly);
```

### 3. Use in Your Service

```csharp
using BW.Libs.Config.Abstractions;

public class MyService
{
    private readonly IVesselFactory _vesselFactory;

    public MyService(IVesselFactory vesselFactory)
    {
        _vesselFactory = vesselFactory;
    }

    public async Task DoWorkAsync()
    {
        var config = await _vesselFactory.GetVesselAsync<DatabaseConfigVessel>();

        // Use rich domain methods
        if (config.ShouldRetry(attemptNumber))
        {
            // Retry logic with config.TimeoutSeconds
        }
    }
}
```

## Configuration JSON Format

Configurations are stored as JSON envelopes with automatic hash computation:

```json
{
  "currentConfigHash": "3F8A9D2E1B4C6F7A",
  "vesselData": {
    "connectionString": "Server=db.example.com;Database=mydb",
    "maxRetries": 3,
    "timeoutSeconds": 30
  }
}
```

**Note**: Hash is automatically computed and updated when saving via `ConfigRepository`.

## DynamoDB Setup

### Create Table

```bash
aws dynamodb create-table \
  --table-name my-config-table \
  --attribute-definitions AttributeName=ConfigClassName,AttributeType=S \
  --key-schema AttributeName=ConfigClassName,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region eu-central-1
```

**Table Schema:**
- **Partition Key**: `ConfigClassName` (String) - Vessel class name
- **Attribute**: `JsonData` (String) - Full JSON envelope

### Seed Initial Config

The vessel class name (e.g., `DatabaseConfigVessel`) is used as the partition key value.

```bash
aws dynamodb put-item \
  --table-name my-config-table \
  --item '{
    "ConfigClassName": {"S": "DatabaseConfigVessel"},
    "JsonData": {"S": "{\"currentConfigHash\": \"\", \"vesselData\": {\"connectionString\": \"...\", \"maxRetries\": 3, \"timeoutSeconds\": 30}}"}
  }'
```

Or use `ConfigRepository` to save (hash auto-computed):

```csharp
var repository = serviceProvider.GetRequiredService<ConfigRepository>();
var envelope = new ConfigEnvelope
{
    VesselData = new
    {
        connectionString = "Server=...",
        maxRetries = 3,
        timeoutSeconds = 30
    }
};
await repository.SaveConfigEnvelopeAsync(nameof(DatabaseConfigVessel), envelope);
```

## Custom Config Sources

Implement `IConfigSource` for custom backends:

```csharp
public interface IConfigSource
{
    Task<string> GetJsonAsync(string configName);
    Task UpdateJsonAsync(string configName, string json);
}
```

Register it:

```csharp
services.AddSingleton<IConfigSource, MyCustomSource>();
services.AddConfigEngine(typeof(MyVessel).Assembly);
```

## How It Works

1. **First Request**: `IVesselFactory.GetVesselAsync<T>()` queries source, deserializes, validates, caches vessel
2. **Subsequent Requests**: Factory checks source hash vs cached hash
3. **Hash Match**: Returns cached vessel (no deserialization overhead)
4. **Hash Differs**: Re-deserializes, re-validates, updates cache
5. **Result**: Runtime config updates without restarting the application

## Advanced Usage

### Manual Config Updates

```csharp
var repository = serviceProvider.GetRequiredService<ConfigRepository>();
var envelope = await repository.GetConfigEnvelopeAsync("DatabaseConfigVessel");

// Modify vesselData
envelope.VesselData = new { /* updated values */ };

// Save (hash auto-computed)
await repository.SaveConfigEnvelopeAsync("DatabaseConfigVessel", envelope);

// Next GetVesselAsync() will pick up changes automatically
```

### Validation Errors

Validation failures throw `ConfigValidationException`:

```csharp
try
{
    var config = await _vesselFactory.GetVesselAsync<DatabaseConfigVessel>();
}
catch (ConfigValidationException ex)
{
    // Log validation error: ex.VesselType, ex.Message
}
```

## Development

### Build
```bash
dotnet build
```

### Pack
```bash
dotnet pack --configuration Release
```

### Publish
```bash
# Get CodeArtifact token
export CODEARTIFACT_AUTH_TOKEN=$(aws codeartifact get-authorization-token \
  --domain bruceware \
  --domain-owner 560719246675 \
  --region eu-central-1 \
  --query authorizationToken \
  --output text)

# Configure NuGet
dotnet nuget update source bruceware-libs \
  --username aws \
  --password $CODEARTIFACT_AUTH_TOKEN \
  --store-password-in-clear-text

# Push package
dotnet nuget push bin/Release/BW.Libs.Config.1.0.0.nupkg --source bruceware-libs
```

## License

Proprietary - BruceWare © 2025
