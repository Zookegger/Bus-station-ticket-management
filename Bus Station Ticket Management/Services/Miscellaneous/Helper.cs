using System.Security.Cryptography;
using System.Text;
namespace Bus_Station_Ticket_Management.Services
{
    public static class Helper
    {
        public static byte[] Hash(string plaintext)
        {
            HashAlgorithm hashAlgorithm = SHA256.Create();
            return hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(plaintext));
        }

        public static string HashHmac512(string plaintext, string? secret)
        {
            if (string.IsNullOrEmpty(plaintext))
                throw new ArgumentNullException(nameof(plaintext), "plaintext cannot be null");

            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException(nameof(secret), "secret cannot be null");

            using (HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}