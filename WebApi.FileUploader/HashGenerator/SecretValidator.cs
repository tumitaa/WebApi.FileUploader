using System.Security.Cryptography;
using System.Text;

namespace WebApi.FileUploader.Services
{
    public class SecretValidator
    {
        private static readonly byte[] key = Encoding.UTF8.GetBytes("0123456789abcdef");
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("abcdef9876543210");
        private static readonly int expirationTimeInSeconds = 5 * 60;

        public static (string secret, bool isValid) DecryptAndValidateHash(string encryptedHash)
        {
            string decryptedText = DecryptString(encryptedHash);
            string[] parts = decryptedText.Split(':');
            string secret = parts[0];
            long timestamp = long.Parse(parts[1]);
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bool isValid = (currentTimestamp - timestamp) <= expirationTimeInSeconds;

            return (secret, isValid);
        }

        private static string DecryptString(string cipherText)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
