namespace Engine
{
    internal class AppFile : IFileSystemEntry
    {
        public string Name { get; }
        public string Path { get; }
        public string? Extension { get; }
        public DateTime Modified { get; }
        public EntryType Type { get; }

        
        public AppFile(string path, string name, DateTime modified, string? extension)
        {
            Name = name;
            Path = path;
            Extension = extension;
            Modified = modified;
            Type = EntryType.File;
        }
        
        public void Open()
        {
            throw new NotImplementedException();
        }

        public bool IsTextFile()
        {
            return Extension == ".txt";
        }
    }
}
