using System.Security.Cryptography;

namespace WebApi.NetCore.Utilities;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string input, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        try
        {
            var parts = expectedHash.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var expectedBytes = Convert.FromBase64String(parts[2]);
            var computedBytes = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, HashAlgorithmName.SHA256, expectedBytes.Length);
            return CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
