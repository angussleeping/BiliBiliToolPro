using System.Security.Cryptography;

namespace Ray.BiliBiliTool.Infrastructure.Helpers;

public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static (string hash, string salt) HashPassword(string password)
    {
        string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
        return (ComputeHash(password, salt), salt);
    }

    public static bool VerifyPassword(string password, string salt, string hash)
    {
        byte[] computed = Convert.FromBase64String(ComputeHash(password, salt));
        byte[] expected = Convert.FromBase64String(hash);

        return CryptographicOperations.FixedTimeEquals(computed, expected);
    }

    private static string ComputeHash(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        return Convert.ToBase64String(hashBytes);
    }
}
