namespace Engine;

internal class AppSearchResult(string path, string name, DateTime modified) : IFileSystemEntry
{
    public string Name { get; } = name;
    public string Path { get; } = path;
    public string? Extension { get; } = null;
    public EntryType Type { get; } = EntryType.SearchResult;
    public DateTime Modified { get; } = modified;

    public void Open()
    {
        throw new NotImplementedException();
    }
}