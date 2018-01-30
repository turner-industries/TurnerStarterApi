using System;
using System.Security.Cryptography;

namespace TurnerStarterApi.Core.Features.Security
{
    public static class TokenGenerator
    {
        public static string Generate(int keyLength = 32)
        {
            var key = new byte[keyLength];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(key);
            }

            var token = Convert.ToBase64String(key)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            return token;
        }
    }
}
