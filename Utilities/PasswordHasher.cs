using System.Security.Cryptography;
using System.Text;

namespace WebApi.NetCore.Utilities;

public static class PasswordHasher
{
    public static string Hash(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }

    public static bool Verify(string input, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        var computedHash = Hash(input);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(expectedHash));
    }
}
