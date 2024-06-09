namespace Engine
{
    public enum EntryType
    {
        Directory,
        File,
        SearchResult,
    }

    public interface IFileSystemEntry
    {
        public string Name { get; }
        public string Path { get; }
        public string? Extension { get; }
        public EntryType Type { get; }
        public DateTime Modified { get; }

        public void Open();
    }
}