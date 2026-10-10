using System;
using System.Security.Cryptography;
using System.Text;

namespace TaskControl.Security
{
    // Tokens for password reset links. The token goes in the e-mailed link; only its hash
    // is stored, so whoever reads the database still cannot build a working link.
    public static class PasswordResetToken
    {
        public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

        // 32 random bytes from the OS cryptographic generator, URL-safe Base64 (43 characters)
        public static string NewToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // SHA-256 as 64 hex characters. The token is already random, so no salt is needed.
        public static string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty)));
    }
}
