namespace BW.Libs.Config.Contracts;

/// <summary>
/// Represents a configuration vessel that can be serialized/deserialized from JSON.
/// Implement as: class MyVessel : IVessel&lt;MyVessel&gt;
/// </summary>
/// <typeparam name="T">The concrete vessel type (self-referencing)</typeparam>
public interface IVessel<T> where T : class, IVessel<T>, new()
{
    /// <summary>
    /// Current hash of the configuration JSON. Empty/null indicates unfilled vessel.
    /// </summary>
    string CurrentConfigHash { get; }

    /// <summary>
    /// Deserializes vessel data from JSON string and populates properties.
    /// </summary>
    /// <param name="vesselDataJson">The 'vesselData' JSON string from config</param>
    void FromJson(string vesselDataJson);

    /// <summary>
    /// Serializes vessel properties to JSON string.
    /// </summary>
    /// <returns>JSON string representing vessel data</returns>
    string ToJson();

    /// <summary>
    /// Validates the current vessel state. Throws exception on validation failure.
    /// </summary>
    void Validate();
}
