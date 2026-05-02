using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using BW.Libs.Config.Abstractions;
using BW.Libs.Config.Contracts;
using BW.Libs.Config.Exceptions;

namespace BW.Libs.Config.Components;

/// <summary>
/// Thread-safe factory for retrieving filled and validated vessels.
/// Manages per-type singleton instances with hash-based change detection.
/// </summary>
internal sealed class VesselFactory : IVesselFactory
{
    private readonly ConfigRepository _repository;
    private readonly ConcurrentDictionary<Type, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<Type, object> _vesselCache = new();

    public VesselFactory(ConfigRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<T> GetVesselAsync<T>() where T : class, IVessel<T>, new()
    {
        var vesselType = typeof(T);
        var configName = vesselType.Name;

        // Get or create lock for this vessel type
        var semaphore = _locks.GetOrAdd(vesselType, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            // Check if we have a cached instance
            var cachedVessel = _vesselCache.TryGetValue(vesselType, out var cached) ? (T)cached : null;

            // Get config envelope from source
            var envelope = await _repository.GetConfigEnvelopeAsync(configName);

            // If cached vessel exists and hash matches, return cached
            if (cachedVessel != null &&
                !string.IsNullOrWhiteSpace(cachedVessel.CurrentConfigHash) &&
                cachedVessel.CurrentConfigHash == envelope.CurrentConfigHash)
            {
                return cachedVessel;
            }

            // Need to create/update vessel
            var vessel = new T();

            // Deserialize vesselData
            if (envelope.VesselData == null)
                throw new ConfigDeserializationException(configName, "VesselData is null in envelope.");

            var vesselDataJson = JsonSerializer.Serialize(envelope.VesselData);

            // Fill vessel
            try
            {
                vessel.FromJson(vesselDataJson);
            }
            catch (Exception ex)
            {
                throw new ConfigDeserializationException(configName, $"FromJson failed: {ex.Message}", ex);
            }

            // Validate vessel
            try
            {
                vessel.Validate();
            }
            catch (Exception ex)
            {
                throw new ConfigValidationException(configName, ex.Message, ex);
            }

            // Update hash via reflection (since CurrentConfigHash is readonly property)
            SetCurrentConfigHash(vessel, envelope.CurrentConfigHash);

            // Cache and return
            _vesselCache[vesselType] = vessel;
            return vessel;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static void SetCurrentConfigHash<T>(T vessel, string hash) where T : class, IVessel<T>, new()
    {
        var property = typeof(T).GetProperty(nameof(IVessel<T>.CurrentConfigHash));
        if (property?.CanWrite == true)
        {
            property.SetValue(vessel, hash);
        }
        else
        {
            // Use backing field if property is init-only or readonly
            var field = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.Name.Contains("CurrentConfigHash", StringComparison.OrdinalIgnoreCase));

            if (field != null)
            {
                field.SetValue(vessel, hash);
            }
        }
    }
}
