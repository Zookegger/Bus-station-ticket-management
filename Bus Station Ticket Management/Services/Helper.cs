using System.Security.Cryptography;
using System.Text;

namespace Bus_Station_Ticket_Management.Service
{
    public static class Helper
    {
        public static byte[] Hash(string plaintext)
        {
            HashAlgorithm hashAlgorithm = SHA256.Create();
            return hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(plaintext));
        }
        public static string HashHmac512(string plaintext, string secret)
        {
            HMACSHA512 hmac = new HMACSHA512(Encoding.ASCII.GetBytes(secret));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(plaintext)));
        }
    }
}
