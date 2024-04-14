namespace Engine;

public interface IFileService
{
    public List<IFileSystemEntry> ListDir(string path);
    public bool IsTextFile(IFileSystemEntry entry);
    public string GetTextFileContent(string filePath);
}