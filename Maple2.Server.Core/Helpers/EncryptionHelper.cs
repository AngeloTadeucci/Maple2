using System.Security.Cryptography;

namespace Maple2.Server.Core.Helpers;

public class EncryptionHelper
{
    public static string HashPassword(string password, byte[] salt)
    {
        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
        {
            byte[] hash = rfc2898DeriveBytes.GetBytes(32); // 32 bytes for SHA-256
            return Convert.ToBase64String(hash);
        }
    }
    
    public static bool VerifyPassword(string enteredPassword, string storedHash, byte[] storedSalt)
    {
        string enteredPasswordHash = HashPassword(enteredPassword, storedSalt);
        
        return storedHash == enteredPasswordHash;
    }

    public static byte[] GenerateSalt()
    {
        var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[16]; // Common salt size (128 bits)
        rng.GetBytes(salt);
        return salt;
    }
}