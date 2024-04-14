﻿namespace Engine
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
                        dir.FullName, dir.Name, dir.LastWriteTime
                        )
                    );
            }
            foreach (var file in files)
            {
                fileSystemEntries.Add(
                    new AppFile(
                        file.FullName, file.Name, file.LastWriteTime, file.Extension
                        )
                    );
            }

            return fileSystemEntries;
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
    }
}
