namespace BW.Libs.Config.Abstractions;

/// <summary>
/// Abstraction for configuration source (DynamoDB, Redis, File, AWS Parameter Store, etc.)
/// </summary>
public interface IConfigSource
{
    /// <summary>
    /// Retrieves configuration JSON by name.
    /// </summary>
    /// <param name="configName">The configuration name (typically vessel class name)</param>
    /// <returns>Full JSON string including hash and vesselData</returns>
    Task<string> GetJsonAsync(string configName);

    /// <summary>
    /// Updates configuration JSON by name.
    /// </summary>
    /// <param name="configName">The configuration name</param>
    /// <param name="json">Full JSON string to persist</param>
    Task UpdateJsonAsync(string configName, string json);
}
