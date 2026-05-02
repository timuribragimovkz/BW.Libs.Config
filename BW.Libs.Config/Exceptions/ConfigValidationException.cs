namespace BW.Libs.Config.Exceptions;

/// <summary>
/// Thrown when vessel validation fails.
/// </summary>
public sealed class ConfigValidationException : Exception
{
    public string VesselType { get; }

    public ConfigValidationException(string vesselType, string message)
        : base($"Validation failed for vessel '{vesselType}': {message}")
    {
        VesselType = vesselType;
    }

    public ConfigValidationException(string vesselType, string message, Exception innerException)
        : base($"Validation failed for vessel '{vesselType}': {message}", innerException)
    {
        VesselType = vesselType;
    }
}
