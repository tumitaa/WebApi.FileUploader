using WebApi.FileUploader.Services;

public class Program
{
    public static void Main()
    {
        string secret = "mySecretKey";
        string encryptedHash = HashGenerator.GenerateHash(secret);
        Console.WriteLine($"Encrypted Hash: {encryptedHash}");
        var result = SecretValidator.DecryptAndValidateHash(encryptedHash);
        Console.WriteLine($"Decrypted Secret: {result.secret}");
        Console.WriteLine($"Is Valid: {result.isValid}");
    }
}
