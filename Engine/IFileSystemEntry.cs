namespace Engine
{
    public enum EntryType
    {
        Directory,
        File,
    }
    public interface IFileSystemEntry
    {
        public string Name { get; }
        public string Path { get; }
        public string? Extension { get; }
        public EntryType Type { get;  }
        public DateTime Modified { get; }

        public void Open();
    }
}
