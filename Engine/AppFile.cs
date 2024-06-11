﻿namespace Engine;

internal class AppFile : IFileSystemEntry
{
    public string Name { get; }
    public string Path { get; }
    public string? Extension { get; }
    public DateTime Modified { get; }
    public DateTime Created { get; }
    public long? Size { get; }
    public EntryType Type { get; }

        
    public AppFile(string path, string name, DateTime modified, DateTime created, string? extension, long? size)
    {
        Name = name;
        Path = path;
        Extension = extension;
        Size = size;
        Modified = modified;
        Created = created;
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