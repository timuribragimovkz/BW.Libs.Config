using System.Text.Json;
using BW.Libs.Config.Abstractions;
using BW.Libs.Config.Contracts;
using BW.Libs.Config.Exceptions;

namespace BW.Libs.Config.Components;

/// <summary>
/// Repository for retrieving and persisting configuration JSON from/to sources.
/// </summary>
internal sealed class ConfigRepository
{
    private readonly IConfigSource _source;

    public ConfigRepository(IConfigSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Gets configuration envelope from source.
    /// </summary>
    public async Task<ConfigEnvelope> GetConfigEnvelopeAsync(string configName)
    {
        var json = await _source.GetJsonAsync(configName);

        if (string.IsNullOrWhiteSpace(json))
            throw new ConfigNotFoundException(configName);

        try
        {
            var envelope = JsonSerializer.Deserialize<ConfigEnvelope>(json);

            if (envelope == null)
                throw new ConfigDeserializationException(configName, "Deserialized envelope is null.");

            return envelope;
        }
        catch (JsonException ex)
        {
            throw new ConfigDeserializationException(configName, ex.Message, ex);
        }
    }

    /// <summary>
    /// Persists configuration envelope to source. Automatically computes and sets the hash.
    /// </summary>
    public async Task SaveConfigEnvelopeAsync(string configName, ConfigEnvelope envelope)
    {
        // Serialize with temp empty hash first
        envelope.CurrentConfigHash = string.Empty;

        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // Compute hash of the entire JSON
        var hash = HashUtility.ComputeHash(json);

        // Update envelope with computed hash
        envelope.CurrentConfigHash = hash;

        // Serialize again with the real hash
        json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await _source.UpdateJsonAsync(configName, json);
    }
}
