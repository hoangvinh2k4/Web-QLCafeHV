using System.Text;
using System.Security.Cryptography;

public static class Encrypt
{    
    public static string SHA256encrypt(string input)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
    }
}
