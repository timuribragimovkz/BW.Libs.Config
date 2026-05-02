using System.Reflection;
using BW.Libs.Config.Contracts;

namespace BW.Libs.Config.Components;

/// <summary>
/// Utility for scanning assemblies to find vessel types.
/// </summary>
internal static class VesselScanner
{
    /// <summary>
    /// Scans provided assemblies for types implementing IVessel&lt;T&gt;.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>Collection of vessel types</returns>
    public static IEnumerable<Type> ScanForVessels(params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            return Enumerable.Empty<Type>();

        return assemblies
            .SelectMany(a => GetTypesFromAssembly(a))
            .Where(IsVesselType)
            .Distinct();
    }

    private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return types that loaded successfully
            return ex.Types.Where(t => t != null)!;
        }
    }

    private static bool IsVesselType(Type type)
    {
        if (!type.IsClass || type.IsAbstract)
            return false;

        return type.GetInterfaces()
            .Any(i => i.IsGenericType &&
                      i.GetGenericTypeDefinition() == typeof(IVessel<>) &&
                      i.GenericTypeArguments.Length == 1 &&
                      i.GenericTypeArguments[0] == type); // Self-referencing check
    }
}
