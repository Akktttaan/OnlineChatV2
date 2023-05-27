using System.Security.Cryptography;
using System.Text;

namespace OnlineChatV2.WebApi.Utilities;

public class CryptoUtilities
{
    public static byte[] GetMd5Hash(string str)
    {
        using var algorithm = MD5.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(str));
    }
    
    public static string GetMd5String(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetMd5Hash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}