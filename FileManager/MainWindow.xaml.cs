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
    public partial class MainWindow
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

            _leftPane = new AppPane(_configManager.GetLeftPaneDefaultLocation(), LeftPaneData, LeftPanePath);
            _rightPane = new AppPane(_configManager.GetRightPaneDefaultLocation(), RightPaneData, RightPanePath);

            LeftPaneData.ItemsSource = _leftPane.Content;
            RightPaneData.ItemsSource = _rightPane.Content;

            _leftPane.FocusOnFirstItem();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            GetPaneToHandle(sender).OpenItem(sender);
        }

        private void Row_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                (ModifierKeys.Control | ModifierKeys.Shift))
            {
                HandleDoubleModifiersShortcuts(sender);
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                HandleSingleModifierShortcuts(sender);
            }
            else
            {
                HandleSingleKeyShortcuts(sender, e);
            }
        }

        private void HandleSingleKeyShortcuts(object sender, KeyEventArgs e)
        {
            var pane = GetPaneToHandle(sender);
            var grid = pane.GetGrid();

            if (e.Key == Key.Enter)
            {
                pane.OpenItem(sender);
            }
            else if (e.Key == Key.Delete)
            {
                ActionHandler.DeleteEntriesAction(pane, grid.SelectedItems);
            }
            else if (e.Key == Key.F1)
            {
                IFileSystemEntry entry = (IFileSystemEntry)grid.SelectedItem;
                if (entry == null)
                {
                    return;
                }

                ActionHandler.OpenPropertiesAction(entry);
            }
            else if (e.Key == Key.F2)
            {
                ActionHandler.RenameEntryAction(pane, sender, this);
            }
            else if (e.Key == Key.F3)
            {
                pane.OpenItemInternally(sender);
            }
        }

        private void HandleSingleModifierShortcuts(object sender)
        {
            var pane = GetPaneToHandle(sender);
            var grid = pane.GetGrid();

            if (Keyboard.IsKeyDown(Key.OemPeriod))
            {
                pane.GoDirForward();
            }

            if (Keyboard.IsKeyDown(Key.OemComma))
            {
                pane.GoDirBack();
            }

            if (Keyboard.IsKeyDown(Key.E))
            {
                ActionHandler.OpenEntryInExplorerAction((IFileSystemEntry)grid.SelectedItem);
            }

            if (Keyboard.IsKeyDown(Key.T))
            {
                ActionHandler.OpenTerminalAction(pane.GetCurrentPath());
            }

            if (Keyboard.IsKeyDown(Key.U))
            {
                ActionHandler.UploadEntryToDropboxAction((IFileSystemEntry)grid.SelectedItem);
            }

            if (Keyboard.IsKeyDown(Key.P))
            {
                _dropboxManager.Auth();
            }

            if (Keyboard.IsKeyDown(Key.Q))
            {
                var qa = new QuickAccessMenu
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
                Keyboard.Focus(pane.GetGridName() == "LeftPaneData"
                    ? LeftPaneSearch.PaneSearchTextBox
                    : RightPaneSearch.PaneSearchTextBox);
            }

            if (Keyboard.IsKeyDown(Key.V))
            {
                if (_itemsToCut.Count > 0)
                {
                    ActionHandler.MoveEntriesAction(_itemsToCut, pane);
                    _itemsToCut.Clear();
                }
                else
                {
                    ActionHandler.CopyEntriesAction(_itemsToCopy, pane);
                }

                pane.Refresh();
            }
        }

        private void HandleDoubleModifiersShortcuts(object sender)
        {
            var pane = GetPaneToHandle(sender);
            if (Keyboard.IsKeyDown(Key.N))
            {
                ActionHandler.CreateDirectoryAction(pane, this);
            }

            if (Keyboard.IsKeyDown(Key.F))
            {
                ActionHandler.DeepSearchDirectoryAction(pane, this);
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                var parentObject = VisualTreeHelper.GetParent(child);

                switch (parentObject)
                {
                    case null:
                        return null;
                    case T parent:
                        return parent;
                    default:
                        child = parentObject;
                        break;
                }
            }
        }

        public AppPane GetPaneToHandle(object sender)
        {
            if (sender is not DataGridRow clickedRow) throw new Exception("Cannot identify sender as DataGridRow");
            var parentDataGrid = FindParent<DataGrid>(clickedRow);

            if (parentDataGrid == null)
            {
                throw new Exception("Could not find parent data grid for clicked row");
            }

            return parentDataGrid.Name switch
            {
                "LeftPaneData" => _leftPane,
                "RightPaneData" => _rightPane,
                _ => throw new Exception("Unknown pane name")
            };
        }

        private void MenuConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var window = new ConfigWindow();
            window.Show();
        }

        private void PaneSearch_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not PaneSearchBox searchBox)
            {
                return;
            }

            if (e.Key != Key.Enter) return;
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

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            var pane = (sender as Button)?.Name switch
            {
                "LeftPaneGoBackButton" => _leftPane,
                "RightPaneGoBackButton" => _rightPane,
                _ => null,
            };

            pane?.GoDirBack();
        }

        private void GoForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var pane = (sender as Button)?.Name switch
            {
                "LeftPaneGoForwardButton" => _leftPane,
                "RightPaneGoForwardButton" => _rightPane,
                _ => null,
            };

            pane?.GoDirForward();
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

            ActionHandler.UploadEntryToDropboxAction((IFileSystemEntry)_lastFocusedDataGrid.SelectedItem);
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
            ActionHandler.UnzipAction(entry, GetPaneByGrid(_lastFocusedDataGrid), this);
        }

        private AppPane GetPaneByGrid(DataGrid grid)
        {
            return grid.Name == "LeftPaneData" ? _leftPane : _rightPane;
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

        private void CreateFolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastFocusedDataGrid == null) return;
            ActionHandler.CreateDirectoryAction(GetPaneByGrid(_lastFocusedDataGrid), this);
        }
    }
}