using System.Security.Cryptography;
using System.Text;

namespace ESILV_A2_S1_ConceptionBD.App;

public static class PasswordHash
{
    public static string Sha256Hex(string salt, string password)
    {
        string input = salt + password;
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);

        var sb = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
