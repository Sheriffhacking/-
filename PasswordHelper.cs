using System;
using System.Security.Cryptography;

namespace SchoolManagementSystem.Security
{
    public static class PasswordHelper
    {
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < hash.Length; i++)
            {
                if (computedHash[i] != hash[i])
                    return false;
            }

            return true;
        }
    }
}