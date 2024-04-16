namespace Engine;

public interface IFileService
{
    public List<IFileSystemEntry> ListDir(string path);
    public bool IsTextFile(IFileSystemEntry entry);
    public string GetTextFileContent(string filePath);
    public IFileSystemEntry GetFileSystemEntryFromDirPath(string path);
    public void DeleteEntry(IFileSystemEntry entry);
    public void CreateDirectory(string path, string name);
}