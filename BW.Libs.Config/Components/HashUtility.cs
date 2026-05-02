using System.Security.Cryptography;
using System.Text;

namespace BW.Libs.Config.Components;

/// <summary>
/// Utility for computing configuration hashes.
/// </summary>
internal static class HashUtility
{
    /// <summary>
    /// Computes SHA256 hash truncated to 16 characters for change detection.
    /// </summary>
    /// <param name="input">Input string to hash</param>
    /// <returns>16-character hex hash</returns>
    public static string ComputeHash(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(bytes);
        string fullHash = Convert.ToHexString(hashBytes);

        return fullHash[..16]; // First 16 characters
    }
}
