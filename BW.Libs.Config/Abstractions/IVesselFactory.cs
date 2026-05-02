using BW.Libs.Config.Contracts;

namespace BW.Libs.Config.Abstractions;

/// <summary>
/// Factory for retrieving filled and validated configuration vessels.
/// Thread-safe with hash-based change detection.
/// </summary>
public interface IVesselFactory
{
    /// <summary>
    /// Gets a filled and validated vessel. Uses cached instance if hash unchanged.
    /// </summary>
    /// <typeparam name="T">The vessel type</typeparam>
    /// <returns>Filled and validated vessel instance</returns>
    Task<T> GetVesselAsync<T>() where T : class, IVessel<T>, new();
}
