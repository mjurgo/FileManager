namespace Engine;

internal class AppSearchResult(string path, string name, DateTime modified, DateTime created) : IFileSystemEntry
{
    public string Name { get; } = name;
    public string Path { get; } = path;
    public string? Extension { get; } = null;
    public EntryType Type { get; } = EntryType.SearchResult;
    public DateTime Modified { get; } = modified;
    public DateTime Created { get; } = created;
    public long? Size { get; set; } = null;

    public void Open()
    {
        throw new NotImplementedException();
    }
}