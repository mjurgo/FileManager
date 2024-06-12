using Dropbox.Api;
using Dropbox.Api.Files;

namespace Engine.Dropbox;

public class DropboxManager
{
    private readonly KeyManager _keyManager = new();
    
    public async Task ListRootFolder(DropboxClient dbx)
    {
        var list = await dbx.Files.ListFolderAsync(String.Empty);

        foreach (var item in list.Entries.Where(i => i.IsFolder))
        {
            Console.WriteLine("D  {0}/", item.Name);
        }

        foreach (var item in list.Entries.Where(i => i.IsFile))
        {
            Console.WriteLine("F{0,8} {1}", item.AsFile.Size, item.Name);
        }
    }

    public bool UploadFile(string filePath, string fileName)
    {
        try
        {
            Task.Run(async () => await Upload(filePath, fileName)).Wait();
        }
        catch (DropboxException)
        {
            // TODO: Add error log
            return false;
        }

        return true;
    }

    private async Task Upload(string filePath, string fileName)
    {
        var token = _keyManager.GetDropboxToken();
        if (token == null)
        {
            Auth();
            token = _keyManager.GetDropboxToken();
        }
        
        byte[] byteArray = File.ReadAllBytes(filePath);

        using var dbx = new DropboxClient(token);
        using (var mem = new MemoryStream(byteArray))
        {
            var uploaded = await dbx.Files.UploadAsync(
                $"/{fileName}",
                WriteMode.Overwrite.Instance,
                body: mem);
        }
    }

    public void Auth()
    {
        var am = new AuthManager();
        Task.Run(async () => await am.GetAccessToken()).Wait();
    }
}