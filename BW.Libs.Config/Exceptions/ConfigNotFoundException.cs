namespace BW.Libs.Config.Exceptions;

/// <summary>
/// Thrown when configuration is not found at the source.
/// </summary>
public sealed class ConfigNotFoundException : Exception
{
    public string ConfigName { get; }

    public ConfigNotFoundException(string configName)
        : base($"Configuration '{configName}' not found at source.")
    {
        ConfigName = configName;
    }

    public ConfigNotFoundException(string configName, Exception innerException)
        : base($"Configuration '{configName}' not found at source.", innerException)
    {
        ConfigName = configName;
    }
}
