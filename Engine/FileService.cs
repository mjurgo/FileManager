using Microsoft.VisualBasic.FileIO;

namespace Engine
{
    public class FileService : IFileService
    {
        private const string DefaultPath = "C:";

        public List<IFileSystemEntry> ListDir(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory {path} doesn't exist.");
            }

            var options = new EnumerationOptions();
            options.ReturnSpecialDirectories = false;
            options.AttributesToSkip = FileAttributes.Hidden | FileAttributes.System;

            DirectoryInfo di = new DirectoryInfo(path);
            var files = di.GetFiles("*", options);
            var dirs = di.GetDirectories("*", options);

            List<IFileSystemEntry> fileSystemEntries = new List<IFileSystemEntry>();

            foreach (var dir in dirs)
            {
                fileSystemEntries.Add(
                    new AppDirectory(
                        dir.FullName, dir.Name, dir.LastWriteTime, dir.CreationTime
                    )
                );
            }

            foreach (var file in files)
            {
                fileSystemEntries.Add(
                    new AppFile(
                        file.FullName, file.Name, file.LastWriteTime, file.CreationTime, file.Extension,
                        (new FileInfo(file.FullName)).Length
                    )
                );
            }

            return fileSystemEntries;
        }

        public IFileSystemEntry GetFileSystemEntryFromDirPath(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return new AppDirectory(
                di.FullName, di.Name, di.LastWriteTime, di.CreationTime
            );
        }

        public void DeleteEntry(IFileSystemEntry entry)
        {
            if (entry.Type == EntryType.File)
            {
                FileSystem.DeleteFile(entry.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            else if (entry.Type == EntryType.Directory)
            {
                FileSystem.DeleteDirectory(entry.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
        }

        public void CreateDirectory(string path, string name)
        {
            string fullPath = $@"{path}\{name}";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public void RenameDirectory(string oldPath, string newPath)
        {
            if (Directory.Exists(oldPath))
            {
                Directory.Move(oldPath, newPath);
            }
        }

        public void RenameFile(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
            }
        }

        public bool IsTextFile(IFileSystemEntry entry)
        {
            if (entry.Extension == null || entry is AppDirectory)
            {
                return false;
            }

            return ((entry as AppFile)!).IsTextFile();
        }

        public string GetTextFileContent(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public List<IFileSystemEntry> GetFileSystemEntriesAsPaths(FileSystemInfo[] items)
        {
            var entries = new List<IFileSystemEntry>();
            foreach (FileSystemInfo item in items)
            {
                if (item is DirectoryInfo)
                {
                    entries.Add(
                        new AppDirectory(
                            item.FullName, item.FullName, item.LastWriteTime, item.CreationTime
                        ));
                }
                else if (item is FileInfo)
                {
                    entries.Add(
                        new AppFile(
                            item.FullName, item.FullName, item.LastWriteTime, item.CreationTime, item.Extension,
                            (new FileInfo(item.FullName)).Length
                        )
                    );
                }
            }

            return entries;
        }

        public IFileSystemEntry CreateSearchResultEntry(string path)
        {
            return new AppSearchResult(path, path, DateTime.Now, DateTime.Now);
        }

        public void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectory(diSourceSubDir, nextTargetSubDir);
            }
        }

        public long GetDirectorySize(DirectoryInfo dir)
        {
            long size = 0;
            FileInfo[] fis = dir.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            DirectoryInfo[] dis = dir.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di);
            }

            return size;
        }
    }
}