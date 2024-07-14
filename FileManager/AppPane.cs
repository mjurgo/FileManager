using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Windows.Controls;
using Engine;
using FileManager.Controls;

namespace FileManager;

public class AppPane
{
    public List<IFileSystemEntry> Content { get; private set; }

    private readonly DataGrid _assignedGrid;
    private readonly PanePathBox _pathBox;

    private readonly IFileService _fileService = new FileService();
    private readonly List<IFileSystemEntry> _viewHistory = [];
    private int _currentDirIndex;

    public AppPane(string path, DataGrid assignedGrid, PanePathBox pathBox)
    {
        _assignedGrid = assignedGrid;
        _pathBox = pathBox;
        Content = _fileService.ListDir(path);
        _viewHistory.Add(_fileService.GetFileSystemEntryFromDirPath(path));
        _pathBox.SetPath(GetCurrentPath());
    }

    public void OpenItem(object sender)
    {
        if (sender is DataGridRow clickedRow)
        {
            IFileSystemEntry? item = clickedRow.Item as IFileSystemEntry;
            if (item == null)
            {
                return;
            }

            if (item.Type == EntryType.Directory)
            {
                OpenDirectory(item);
            }
            else if (item.Type == EntryType.File)
            {
                OpenFile(item);
            }
        }
    }

    public void OpenPath(string path)
    {
        Content = _fileService.ListDir(path);
        _assignedGrid.ItemsSource = Content;
        if (_currentDirIndex < _viewHistory.Count - 1)
        {
            _viewHistory.RemoveRange(_currentDirIndex + 1, _viewHistory.Count - _currentDirIndex - 1);
        }
        _viewHistory.Add(_fileService.GetFileSystemEntryFromDirPath(path));
        _currentDirIndex++;
        Refresh();
    }

    private void OpenDirectory(IFileSystemEntry item)
    {
        Content = (new FileService()).ListDir(item.Path);
        _assignedGrid.ItemsSource = Content;
        if (_currentDirIndex < _viewHistory.Count - 1)
        {
            _viewHistory.RemoveRange(_currentDirIndex + 1, _viewHistory.Count - _currentDirIndex - 1);
        }

        _viewHistory.Add(item);
        _currentDirIndex++;
        Refresh();
    }

    private static void OpenFile(IFileSystemEntry item)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = item.Path,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception e)
        {
            AppLogger.Error($"error while opening file: {e.Message}");
        }
    }

    public void GoDirForward()
    {
        if (!CanGoDirForward()) return;
        Content = _fileService.ListDir(_viewHistory[_currentDirIndex + 1].Path);
        _currentDirIndex++;
        _assignedGrid.ItemsSource = Content;
        _pathBox.SetPath(GetCurrentPath());
    }

    public void GoDirBack()
    {
        if (!CanGoDirBack()) return;
        if (_viewHistory[_currentDirIndex].Type == EntryType.SearchResult)
        {
            _viewHistory.Remove(_viewHistory[_currentDirIndex]);
        }
        Content = _fileService.ListDir(_viewHistory[_currentDirIndex - 1].Path);
        _currentDirIndex--;
        _assignedGrid.ItemsSource = Content;
        _pathBox.SetPath(GetCurrentPath());
    }

    private bool CanGoDirBack()
    {
        return _currentDirIndex > 0;
    }

    private bool CanGoDirForward()
    {
        return _currentDirIndex < _viewHistory.Count - 1;
    }

    public void DeleteEntryAsRow(object sender)
    {
        if (sender is DataGridRow clickedRow)
        {
            IFileSystemEntry? item = clickedRow.Item as IFileSystemEntry;
            if (item == null)
            {
                return;
            }

            _fileService.DeleteEntry(item);
        }
    }

    public void DeleteEntry(IFileSystemEntry entry)
    {
        _fileService.DeleteEntry(entry);
    }

    public void Refresh()
    {
        _assignedGrid.ItemsSource = _fileService.ListDir(_viewHistory[_currentDirIndex].Path);
        _pathBox.SetPath(GetCurrentPath());
        FocusOnFirstItem();
    }

    public void CreateDirectory(string name)
    {
        _fileService.CreateDirectory(_viewHistory[_currentDirIndex].Path, name);
    }

    public void RenameEntry(object sender, string newName)
    {
        if (sender is DataGridRow targetRow)
        {
            IFileSystemEntry? item = targetRow.Item as IFileSystemEntry;
            if (item == null)
            {
                return;
            }

            var newPath = _viewHistory[_currentDirIndex].Path + "\\" + newName;

            switch (item.Type)
            {
                case EntryType.Directory:
                    _fileService.RenameDirectory(item.Path, newPath);
                    break;
                case EntryType.File:
                    _fileService.RenameFile(item.Path, newPath);
                    break;
            }
        }
    }

    public void FocusOnFirstItem()
    {
        _assignedGrid.SelectedIndex = 0;
        _assignedGrid.Focus();
    }

    public void FocusOnItem(int index)
    {
        _assignedGrid.SelectedIndex = index;
        _assignedGrid.Focus();
    }

    public void OpenItemInternally(object sender)
    {
        if (sender is DataGridRow clickedRow)
        {
            IFileSystemEntry? item = clickedRow.Item as IFileSystemEntry;
            if (item == null)
            {
                return;
            }

            if (item.Type == EntryType.File)
            {
                OpenFileInternally(item);
            }
        }
    }

    private void OpenFileInternally(IFileSystemEntry item)
    {
        if (!_fileService.IsTextFile(item))
        {
            return;
        }

        TextFileViewWindow window = new TextFileViewWindow(item.Name)
        {
            Content =
            {
                Text = _fileService.GetTextFileContent(item.Path)
            }
        };
        window.Show();
    }

    public bool FindItemInCurrentLocation(string name)
    {
        var itemIndex = FindItemIndexInCurrentLocation(name);
        if (itemIndex == null)
        {
            return false;
        }

        FocusOnItem((int)itemIndex);
        return true;
    }

    public bool FindItem(string name)
    {
        var di = new DirectoryInfo(_viewHistory[_currentDirIndex].Path);
        var items = di.GetFileSystemInfos($"*{name}*", SearchOption.AllDirectories);

        _assignedGrid.ItemsSource = _fileService.GetFileSystemEntriesAsPaths(items);
        _viewHistory.Add(_fileService.CreateSearchResultEntry(_viewHistory[_currentDirIndex].Path));
        _currentDirIndex++;

        return items.Length > 0;
    }

    private int? FindItemIndexInCurrentLocation(string name)
    {
        for (int i = 0; i < Content.Count; i++)
        {
            if (Content[i].Name == name)
            {
                return i;
            }
        }

        return null;
    }

    public string GetGridName()
    {
        return _assignedGrid.Name;
    }

    public DataGrid GetGrid()
    {
        return _assignedGrid;
    }

    public string GetCurrentPath()
    {
        return _viewHistory[_currentDirIndex].Path;
    }
}