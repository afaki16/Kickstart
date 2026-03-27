using System;
using System.Security.Cryptography;
using System.Text;

namespace Kickstart.Infrastructure.Security
{
    /// <summary>
    /// SHA-256 over UTF-8 bytes of the plaintext refresh token, stored as Base64 (same algorithm as PostgreSQL migration).
    /// </summary>
    public static class RefreshTokenHasher
    {
        public static string Hash(string plainToken)
        {
            if (plainToken is null)
                throw new ArgumentNullException(nameof(plainToken));

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
            return Convert.ToBase64String(bytes);
        }
    }
}
