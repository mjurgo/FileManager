using System.IO;
using Engine;

namespace FileManager;

internal class EntryPropertiesViewModel
{
        public string? Name { get; set; }
        public string? Path { get; set; }
        public EntryType Type { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public long? Size { get; set; }
}

public partial class EntryPropertiesWindow
{
    private readonly IFileService _fileService;
    public EntryPropertiesWindow(IFileSystemEntry entry)
    {
        _fileService = new FileService();
        InitializeComponent();

        var properties = new EntryPropertiesViewModel
        {
            Name = entry.Name,
            Path = entry.Path,
            Type = entry.Type,
            Modified = entry.Modified,
            Created = entry.Created,
            Size = entry.Size,
        };

        if (entry.Type == EntryType.Directory)
        {
            properties.Size = _fileService.GetDirectorySize(new DirectoryInfo(entry.Path));
        }

        DataContext = properties;
        Title = $"Properties: {entry.Name}";
    }
}