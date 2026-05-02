using System.Text.Json.Serialization;

namespace BW.Libs.Config.Contracts;

/// <summary>
/// Envelope structure for configuration JSON at source.
/// </summary>
public sealed class ConfigEnvelope
{
    /// <summary>
    /// Hash of the entire JSON object. Changes when any data changes.
    /// </summary>
    [JsonPropertyName("currentConfigHash")]
    public string CurrentConfigHash { get; set; } = string.Empty;

    /// <summary>
    /// The actual vessel configuration data.
    /// </summary>
    [JsonPropertyName("vesselData")]
    public object? VesselData { get; set; }
}
