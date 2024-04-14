namespace Engine
{
    internal class AppDirectory : IFileSystemEntry
    {
        public string Name { get; }
        public string Path { get; }
        public DateTime Modified { get; }
        public EntryType Type { get; }


        public AppDirectory(string path, string name, DateTime modified)
        {
            Name = name;
            Path = path;
            Modified = modified;
            Type = EntryType.Directory;
        }
        
        public void Open()
        {
            throw new NotImplementedException();
        }
    }
}
