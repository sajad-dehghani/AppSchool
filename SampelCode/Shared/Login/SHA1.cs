using System;
using System.Security.Cryptography;
using System.Text;

namespace NovinApp.Shared.Login
{
    public class SHA1 : ISHA1
    {
        // Decode Base64 to string
        public string Decrypt(string content)
        {
            try
            {
                byte[] data = Convert.FromBase64String(content.Trim());
                string result = Encoding.UTF8.GetString(data);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Hash + Base64 encode
        public string Encript(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
