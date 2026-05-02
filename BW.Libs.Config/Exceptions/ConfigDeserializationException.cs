namespace BW.Libs.Config.Exceptions;

/// <summary>
/// Thrown when configuration JSON cannot be deserialized.
/// </summary>
public sealed class ConfigDeserializationException : Exception
{
    public string ConfigName { get; }

    public ConfigDeserializationException(string configName, string message)
        : base($"Failed to deserialize configuration '{configName}': {message}")
    {
        ConfigName = configName;
    }

    public ConfigDeserializationException(string configName, string message, Exception innerException)
        : base($"Failed to deserialize configuration '{configName}': {message}", innerException)
    {
        ConfigName = configName;
    }
}
