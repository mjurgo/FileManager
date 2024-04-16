using System.Windows.Controls;
using Engine;

namespace FileManager;

public class AppPane
{
    public List<IFileSystemEntry> Content { get; private set; }

    private readonly DataGrid _assignedGrid;
    
    private readonly IFileService _fileService = new FileService();
    private readonly List<IFileSystemEntry> _viewHistory = new List<IFileSystemEntry>();
    private int _currentDirIndex;

    public AppPane(string path, DataGrid assignedGrid)
    {
        _assignedGrid = assignedGrid;
        Content = _fileService.ListDir(path);
        _viewHistory.Add(_fileService.GetFileSystemEntryFromDirPath(path));
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
        }

        private void OpenFile(IFileSystemEntry item)
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
        
        public void GoDirForward()
        {
            if (CanGoDirForward())
            {
                Content = _fileService.ListDir(_viewHistory[_currentDirIndex + 1].Path);
                _currentDirIndex++;
                _assignedGrid.ItemsSource = Content;
            }
        }

        public void GoDirBack()
        {
            if (CanGoDirBack())
            {
                Content = _fileService.ListDir(_viewHistory[_currentDirIndex - 1].Path);
                _currentDirIndex--;
                _assignedGrid.ItemsSource = Content;
            }
        }
        
        private bool CanGoDirBack()
        {
            return _currentDirIndex > 0;
        }

        private bool CanGoDirForward()
        {
            return _currentDirIndex < _viewHistory.Count - 1;
        }

        public void DeleteEntry(object sender)
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

        public void Refresh()
        {
            _assignedGrid.ItemsSource = _fileService.ListDir(_viewHistory[_currentDirIndex].Path);
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
}