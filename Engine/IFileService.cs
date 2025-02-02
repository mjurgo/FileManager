﻿namespace Engine;

public interface IFileService
{
    public List<IFileSystemEntry> ListDir(string path);
    public bool IsTextFile(IFileSystemEntry entry);
    public string GetTextFileContent(string filePath);
    public IFileSystemEntry GetFileSystemEntryFromDirPath(string path);
    public void DeleteEntry(IFileSystemEntry entry);
    public void CreateDirectory(string path, string name);
    void RenameDirectory(string oldPath, string newPath);
    void RenameFile(string oldPath, string newPath);
    public List<IFileSystemEntry> GetFileSystemEntriesAsPaths(FileSystemInfo[] items);
    public IFileSystemEntry CreateSearchResultEntry(string path);
    public void CopyDirectory(DirectoryInfo source, DirectoryInfo target);
    public long GetDirectorySize(DirectoryInfo dir);
    public void UnzipFile(IFileSystemEntry file, string targetPath);
    public void ZipDirectory(IFileSystemEntry directory, string targetFile, string currentPath);
}