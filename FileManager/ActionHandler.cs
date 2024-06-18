using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Engine;
using Engine.Dropbox;

namespace FileManager;

public class ActionHandler
{
    private static readonly DropboxManager DropboxManager = new();
    private static readonly IFileService FileService = new FileService();

    public static void DeleteEntriesAction(AppPane pane, IList? selectedItems)
    {
        if (selectedItems is null)
        {
            return;
        }

        var msg = selectedItems.Count > 1
            ? $"Are you sure you want to delete multiple items ({selectedItems.Count})?"
            : "Are you sure you want to delete this item?";

        var confirmed = MessageBox.Show(
            msg,
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirmed != MessageBoxResult.Yes) return;
        foreach (IFileSystemEntry entry in selectedItems)
        {
            pane.DeleteEntry(entry);
        }

        pane.Refresh();
    }

    public static void RenameEntryAction(AppPane pane, object sender, Window owner)
    {
        var inputWindow = new InputDialogWindow("Enter a new name for the item")
        {
            Owner = owner
        };
        if (inputWindow.ShowDialog() != true) return;
        var input = inputWindow.InputText;
        pane.RenameEntry(sender, input);
        pane.Refresh();
    }

    public static void OpenPropertiesAction(IFileSystemEntry entry)
    {
        EntryPropertiesWindow propertiesWindow =
            new EntryPropertiesWindow(entry);
        propertiesWindow.Show();
    }

    public static void OpenEntryInExplorerAction(IFileSystemEntry entry)
    {
        if (entry.Type != EntryType.Directory)
        {
            return;
        }

        Process.Start("explorer.exe", entry.Path);
    }

    public static void OpenTerminalAction(string path)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            WorkingDirectory = path
        };
        Process.Start(processStartInfo);
    }

    public static void UploadEntryToDropboxAction(IFileSystemEntry entry)
    {
        if (entry.Type != EntryType.File)
        {
            return;
        }

        if (!DropboxManager.UploadFile(entry.Path, entry.Name))
        {
            MessageBox.Show("Couldn't upload file.", "Upload failed", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        MessageBox.Show("File uploaded successfully.", "Upload successful", MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public static void MoveEntriesAction(List<IFileSystemEntry> entries, AppPane pane)
    {
        if (entries.Count <= 0) return;
        foreach (IFileSystemEntry item in entries)
        {
            if (item.Type == EntryType.File)
            {
                File.Move(item.Path, @$"{pane.GetCurrentPath()}\{item.Name}");
            }
            else if (item.Type == EntryType.Directory)
            {
                Directory.Move(item.Path, @$"{pane.GetCurrentPath()}\{item.Name}");
            }
        }
    }

    public static void CopyEntriesAction(List<IFileSystemEntry> entries, AppPane pane)
    {
        if (entries.Count <= 0) return;
        foreach (IFileSystemEntry item in entries)
        {
            if (item.Type == EntryType.File)
            {
                File.Copy(item.Path, @$"{pane.GetCurrentPath()}\{item.Name}");
            }
            else if (item.Type == EntryType.Directory)
            {
                var sourceDir = new DirectoryInfo(item.Path);
                var targetDir = new DirectoryInfo(@$"{pane.GetCurrentPath()}\{item.Name}");
                FileService.CopyDirectory(sourceDir, targetDir);
            }
        }
    }

    public static void CreateDirectoryAction(AppPane pane, Window owner)
    {
        var inputWindow = new InputDialogWindow("Enter the name for new directory")
        {
            Owner = owner
        };
        if (inputWindow.ShowDialog() != true) return;
        var input = inputWindow.InputText;
        if (input == string.Empty) return;
        pane.CreateDirectory(input);
        pane.Refresh();
    }

    public static void DeepSearchDirectoryAction(AppPane pane, Window owner)
    {
        var inputWindow = new InputDialogWindow("Enter name of the file to search for:")
        {
            Owner = owner,
            Title = "Find file"
        };
        if (inputWindow.ShowDialog() != true) return;
        var input = inputWindow.InputText;
        if (!pane.FindItem(input))
        {
            MessageBox.Show("Item not found in current and nested directories.", "Not found",
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
        }
    }

    public static void UnzipAction(IFileSystemEntry entry, AppPane pane, Window owner)
    {
        if (entry.Type != EntryType.File || entry.Extension != ".zip")
        {
            MessageBox.Show("Cannot unzip file that isn't a zip.", "Can't unzip", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var inputWindow = new InputDialogWindow("Enter the name of target directory")
        {
            Owner = owner
        };
        if (inputWindow.ShowDialog() != true) return;
        var input = inputWindow.InputText;
        if (input == string.Empty)
        {
            input = entry.Name.Replace(".zip", "");
        }

        FileService.UnzipFile(entry, input);
        pane.Refresh();
    }
}