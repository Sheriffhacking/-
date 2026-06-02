using System;
using System.Security.Cryptography;
using System.Text;

namespace SchoolManagementSystem.Security
{
    public static class PasswordHelper
    {
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computed.Length; i++)
            {
                if (computed[i] != hash[i])
                    return false;
            }

            return true;
        }
    }
}