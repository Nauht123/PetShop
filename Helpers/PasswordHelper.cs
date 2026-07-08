// Helpers/PasswordHelper.cs
using System.Security.Cryptography;
using System.Text;

namespace PetShop.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}