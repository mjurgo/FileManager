using System.Text;
using System.Security.Cryptography;

namespace Engine.Dropbox;

public class KeyManager
{
    public string GetDropBoxKey()
    {
        var envSource = ".env";
        var lines = File.ReadAllLines(envSource);
        foreach (string line in lines)
        {
            if (line.StartsWith("DROPBOX_KEY="))
            {
                return line.Substring("DROPBOX_KEY=".Length).Trim();
            }
        }

        throw new Exception("Dropbox key not found in env file.");
    }

    public void SaveDropboxToken(string token)
    {
        byte[] tokenBytes = Encoding.ASCII.GetBytes(token);
        var encrypted = ProtectedData.Protect(tokenBytes, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(".dropbox_token", encrypted);
    }

    public string? GetDropboxToken()
    {
        try
        {
            byte[] tokenBytes = File.ReadAllBytes(".dropbox_token");
            if (tokenBytes.Length <= 0)
            {
                return null;
            }

            var decrypted = ProtectedData.Unprotect(tokenBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.ASCII.GetString(decrypted);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}