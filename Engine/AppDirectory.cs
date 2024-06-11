namespace Engine;

internal class AppDirectory : IFileSystemEntry
{
    public string Name { get; }
    public string Path { get; }
    public string? Extension { get; }
    public DateTime Modified { get; }
    public DateTime Created { get; }
    public long? Size { get; } = null;
    public EntryType Type { get; }


    public AppDirectory(string path, string name, DateTime modified, DateTime created)
    {
        Name = name;
        Path = path;
        Extension = null;
        Modified = modified;
        Created = created;
        Type = EntryType.Directory;
    }

    public void Open()
    {
        throw new NotImplementedException();
    }
}