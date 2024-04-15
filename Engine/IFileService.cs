using System.Collections.ObjectModel;

namespace Engine;

public interface IFileService
{
    public List<IFileSystemEntry> ListDir(string path);
    public bool IsTextFile(IFileSystemEntry entry);
    public ObservableCollection<string> GetTextFileContent(string filePath);
}