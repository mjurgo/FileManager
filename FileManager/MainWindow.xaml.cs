﻿using System.Diagnostics;
using System.IO;
using Engine;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Engine.Config;
using Engine.Dropbox;
using FileManager.Controls;

namespace FileManager
{
    public partial class MainWindow : Window
    {
        private readonly AppPane _leftPane;
        private readonly AppPane _rightPane;
        private readonly ConfigManager _configManager;
        private readonly IFileService _fileService;
        private readonly DropboxManager _dropboxManager;
        private DataGrid? _lastFocusedDataGrid;

        private readonly List<IFileSystemEntry> _itemsToCut = [];
        private readonly List<IFileSystemEntry> _itemsToCopy = [];

        public MainWindow()
        {
            InitializeComponent();

            _configManager = new ConfigManager();
            _fileService = new FileService();
            _dropboxManager = new DropboxManager();

            AppTheme.ChangeTheme(_configManager.GetCurrentTheme());

            _leftPane = new AppPane(_configManager.GetLeftPaneDefaultLocation(), LeftPaneData);
            _rightPane = new AppPane(_configManager.GetRightPaneDefaultLocation(), RightPaneData);

            LeftPaneData.ItemsSource = _leftPane.Content;
            RightPaneData.ItemsSource = _rightPane.Content;

            _leftPane.FocusOnFirstItem();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var paneToHandle = GetPaneToHandle(sender);
            paneToHandle.OpenItem(sender);
        }

        private void Row_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (Keyboard.IsKeyDown(Key.N))
                {
                    InputDialogWindow inputWindow = new InputDialogWindow("Enter the name for new directory");
                    inputWindow.Owner = this;
                    if (inputWindow.ShowDialog() == true)
                    {
                        string input = inputWindow.InputText;
                        var pane = GetPaneToHandle(sender);
                        pane.CreateDirectory(input);
                        pane.Refresh();
                    }
                }

                if (Keyboard.IsKeyDown(Key.F))
                {
                    InputDialogWindow inputWindow = new InputDialogWindow("Enter name of the file to search for:");
                    inputWindow.Owner = this;
                    inputWindow.Title = "Find file";
                    if (inputWindow.ShowDialog() == true)
                    {
                        string input = inputWindow.InputText;
                        var pane = GetPaneToHandle(sender);
                        bool found = pane.FindItem(input);
                        if (!found)
                        {
                            MessageBox.Show("Item not found in current and nested directories.", "Not found",
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                        }
                    }
                }
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Keyboard.IsKeyDown(Key.OemPeriod))
                {
                    GetPaneToHandle(sender).GoDirForward();
                }

                if (Keyboard.IsKeyDown(Key.OemComma))
                {
                    GetPaneToHandle(sender).GoDirBack();
                }

                if (Keyboard.IsKeyDown(Key.E))
                {
                    var pane = GetPaneToHandle(sender);
                    var grid = pane.GetGrid();
                    IFileSystemEntry entry = (IFileSystemEntry)grid.SelectedItem;
                    if (entry.Type != EntryType.Directory)
                    {
                        return;
                    }

                    Process.Start("explorer.exe", entry.Path);
                }

                if (Keyboard.IsKeyDown(Key.T))
                {
                    var pane = GetPaneToHandle(sender);
                    var path = pane.GetCurrentPath();
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        WorkingDirectory = path
                    };
                    Process.Start(processStartInfo);
                }

                if (Keyboard.IsKeyDown(Key.U))
                {
                    var pane = GetPaneToHandle(sender);
                    var grid = pane.GetGrid();
                    IFileSystemEntry entry = (IFileSystemEntry)grid.SelectedItem;
                    if (entry.Type != EntryType.File)
                    {
                        return;
                    }

                    if (!_dropboxManager.UploadFile(entry.Path, entry.Name))
                    {
                        MessageBox.Show("Couldn't upload file.", "Upload failed", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show("File uploaded successfully.", "Upload successful", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                if (Keyboard.IsKeyDown(Key.P))
                {
                    _dropboxManager.Auth();
                }

                if (Keyboard.IsKeyDown(Key.Q))
                {
                    var pane = GetPaneToHandle(sender);
                    QuickAccessMenu qa = new QuickAccessMenu
                    {
                        IsOpen = true,
                        PlacementTarget = pane.GetGrid(),
                        Placement = PlacementMode.Relative,
                        HorizontalOffset = 0,
                        VerticalOffset = 0,
                    };
                }

                if (Keyboard.IsKeyDown(Key.F))
                {
                    var pane = GetPaneToHandle(sender);
                    Keyboard.Focus(pane.GetGridName() == "LeftPaneData"
                        ? LeftPaneSearch.PaneSearchTextBox
                        : RightPaneSearch.PaneSearchTextBox);
                }

                if (Keyboard.IsKeyDown(Key.V))
                {
                    var pane = GetPaneToHandle(sender);
                    if (_itemsToCut.Count > 0)
                    {
                        foreach (IFileSystemEntry item in _itemsToCut)
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

                        _itemsToCut.Clear();
                    }
                    else
                    {
                        foreach (IFileSystemEntry item in _itemsToCopy)
                        {
                            if (item.Type == EntryType.File)
                            {
                                File.Copy(item.Path, @$"{pane.GetCurrentPath()}\{item.Name}");
                            }
                            else if (item.Type == EntryType.Directory)
                            {
                                var sourceDir = new DirectoryInfo(item.Path);
                                var targetDir = new DirectoryInfo(@$"{pane.GetCurrentPath()}\{item.Name}");
                                _fileService.CopyDirectory(sourceDir, targetDir);
                            }
                        }
                    }

                    pane.Refresh();
                }
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    GetPaneToHandle(sender).OpenItem(sender);
                }
                else if (e.Key == Key.Delete)
                {
                    var pane = GetPaneToHandle(sender);
                    var selectedItems = pane.GetGrid().SelectedItems;
                    var msg = selectedItems.Count > 1
                        ? $"Are you sure you want to delete multiple items ({selectedItems.Count})?"
                        : "Are you sure you want to delete this item?";

                    var confirmed = MessageBox.Show(
                        msg,
                        "Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (confirmed == MessageBoxResult.Yes)
                    {
                        foreach (IFileSystemEntry entry in selectedItems)
                        {
                            pane.DeleteEntry(entry);
                        }

                        pane.Refresh();
                    }
                }
                else if (e.Key == Key.F1)
                {
                    var pane = GetPaneToHandle(sender);
                    var grid = pane.GetGrid();
                    IFileSystemEntry entry = (IFileSystemEntry)grid.SelectedItem;
                    if (entry == null)
                    {
                        return;
                    }

                    EntryPropertiesWindow propertiesWindow =
                        new EntryPropertiesWindow(entry);
                    propertiesWindow.Show();
                }
                else if (e.Key == Key.F2)
                {
                    InputDialogWindow inputWindow = new InputDialogWindow("Enter a new name for the item");
                    inputWindow.Owner = this;
                    if (inputWindow.ShowDialog() == true)
                    {
                        string input = inputWindow.InputText;
                        var pane = GetPaneToHandle(sender);
                        pane.RenameEntry(sender, input);
                        pane.Refresh();
                    }
                }
                else if (e.Key == Key.F3)
                {
                    GetPaneToHandle(sender).OpenItemInternally(sender);
                }
            }
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T? parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        public AppPane GetPaneToHandle(object sender)
        {
            if (sender is DataGridRow clickedRow)
            {
                DataGrid? parentDataGrid = FindParent<DataGrid>(clickedRow);

                if (parentDataGrid == null)
                {
                    throw new Exception("Could not find parent data grid for clicked row");
                }

                if (parentDataGrid.Name == "LeftPaneData")
                {
                    return _leftPane;
                }

                if (parentDataGrid.Name == "RightPaneData")
                {
                    return _rightPane;
                }

                throw new Exception("Unknown pane name");
            }

            throw new Exception("Cannot identify sender as DataGridRow");
        }

        private void MenuConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow window = new ConfigWindow();
            window.Show();
        }

        private void PaneSearch_OnKeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Add proper logs in case of errors
            if (sender is not PaneSearchBox searchBox)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                var pane = searchBox.Name switch
                {
                    "LeftPaneSearch" => _leftPane,
                    "RightPaneSearch" => _rightPane,
                    _ => null,
                };
                if (pane == null)
                {
                    return;
                }

                var itemFound = pane.FindItemInCurrentLocation(searchBox.PaneSearchTextBox.Text);
                if (!itemFound)
                {
                    MessageBox.Show("Item not found in current location.", "Not found", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
            }
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            var pane = (sender as Button).Name switch
            {
                "LeftPaneGoBackButton" => _leftPane,
                "RightPaneGoBackButton" => _rightPane,
                _ => null,
            };
            if (pane == null)
            {
                return;
            }

            pane.GoDirBack();
        }

        private void GoForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var pane = (sender as Button).Name switch
            {
                "LeftPaneGoForwardButton" => _leftPane,
                "RightPaneGoForwardButton" => _rightPane,
                _ => null,
            };
            if (pane == null)
            {
                return;
            }

            pane.GoDirForward();
        }

        public void AddItemToCut(IFileSystemEntry item)
        {
            _itemsToCut.Add(item);
        }

        public void AddItemToCopy(IFileSystemEntry item)
        {
            _itemsToCopy.Add(item);
        }

        public void ClearFilesToCut()
        {
            _itemsToCut.Clear();
        }

        public void ClearFilesToCopy()
        {
            _itemsToCopy.Clear();
        }

        public AppPane GetLeftPane()
        {
            return _leftPane;
        }

        public AppPane GetRightPane()
        {
            return _rightPane;
        }

        private void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastFocusedDataGrid == null)
            {
                MessageBox.Show("Couldn't retrieve selected item.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            IFileSystemEntry entry = (IFileSystemEntry)_lastFocusedDataGrid.SelectedItem;
            if (entry.Type != EntryType.File)
            {
                return;
            }

            if (!_dropboxManager.UploadFile(entry.Path, entry.Name))
            {
                MessageBox.Show("Couldn't upload file.", "Upload failed", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("File uploaded successfully.", "Upload successful", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void DataGrid_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _lastFocusedDataGrid = sender as DataGrid;
        }

        private void AuthButton_OnClick(object sender, RoutedEventArgs e)
        {
            _dropboxManager.Auth();
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UnzipButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastFocusedDataGrid == null)
            {
                MessageBox.Show("Couldn't retrieve selected item.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            IFileSystemEntry entry = (IFileSystemEntry)_lastFocusedDataGrid.SelectedItem;
            if (entry.Type != EntryType.File || entry.Extension != ".zip")
            {
                MessageBox.Show("Cannot unzip file that isn't a zip.", "Can't unzip", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var inputWindow = new InputDialogWindow("Enter the name of target directory")
            {
                Owner = this
            };
            if (inputWindow.ShowDialog() != true) return;
            var input = inputWindow.InputText;
            if (input == string.Empty)
            {
                input = entry.Name.Replace(".zip", "");
            }

            _fileService.UnzipFile(entry, input);
            GetPaneByGrid(_lastFocusedDataGrid).Refresh();
        }

        private AppPane GetPaneByGrid(DataGrid grid)
        {
            if (grid.Name == "LeftPaneData")
            {
                return _leftPane;
            }

            return _rightPane;
        }

        private void ZipButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastFocusedDataGrid == null)
            {
                MessageBox.Show("Couldn't retrieve selected item.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            IFileSystemEntry entry = (IFileSystemEntry)_lastFocusedDataGrid.SelectedItem;
            if (entry.Type != EntryType.Directory)
            {
                MessageBox.Show("Must be a directory", "Can't compress", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var inputWindow = new InputDialogWindow("Enter the name of target file")
            {
                Owner = this
            };
            if (inputWindow.ShowDialog() != true) return;
            var input = inputWindow.InputText;
            if (input == string.Empty)
            {
                input = entry.Name;
            }

            var pane = GetPaneByGrid(_lastFocusedDataGrid);
            _fileService.ZipDirectory(entry, input, pane.GetCurrentPath());
            pane.Refresh();
        }
    }
}