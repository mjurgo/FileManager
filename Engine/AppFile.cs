﻿namespace Engine
{
    internal class AppFile : IFileSystemEntry
    {
        public string Name { get; }
        public string Path { get; }
        public DateTime Modified { get; }
        public EntryType Type { get; }

        
        public AppFile(string path, string name, DateTime modified)
        {
            Name = name;
            Path = path;
            Modified = modified;
            Type = EntryType.File;
        }
        
        public void Open()
        {
            throw new NotImplementedException();
        }
    }
}
