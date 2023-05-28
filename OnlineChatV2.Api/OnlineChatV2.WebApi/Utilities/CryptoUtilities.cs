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

    public static byte[] GetMd5Hash(long x, long y)
    {
        var xorBytes = BitConverter.GetBytes(x ^ y);
        using var md5 = MD5.Create();
        return md5.ComputeHash(xorBytes);
    }

    public static string GetMd5String(long x, long y)
    {
        var sb = new StringBuilder();
        foreach (var b in GetMd5Hash(x, y))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
    
    public static string GetMd5String(string inputString)
    {
        var sb = new StringBuilder();
        foreach (var b in GetMd5Hash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}