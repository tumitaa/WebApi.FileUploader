using System.Security.Cryptography;
using System.Text;

public class HashGenerator
{
    private static readonly byte[] key = Encoding.UTF8.GetBytes("0123456789abcdef");
    private static readonly byte[] iv = Encoding.UTF8.GetBytes("abcdef9876543210");

    public static string GenerateHash(string secret)
    {
        return EncryptString(secret);
    }
    public static long GetUnixTimestamp()
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(DateTime.UtcNow - epoch).TotalSeconds;
    }

    private static string EncryptString(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(plainText);
                    }
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}
