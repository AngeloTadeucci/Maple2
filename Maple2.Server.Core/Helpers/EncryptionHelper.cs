namespace Maple2.Server.Core.Helpers;

public class EncryptionHelper {
    public static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, 13);

    public static bool VerifyPassword(string enteredPassword, string storedHash) => BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
}
