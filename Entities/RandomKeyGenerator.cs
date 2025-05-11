using System;
using System.Security.Cryptography;

namespace Entities
{
    public static class RandomKeyGenerator
    {
        public static string GenerateHS256Key()
        {
            byte[] key = new byte[32]; // 256 bits = 32 bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key); // Return the key as a Base64 string
        }
    }
}
