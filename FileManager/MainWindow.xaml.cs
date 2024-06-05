using System.Diagnostics;
using Engine;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Engine.Config;
using FileManager.Controls;

namespace FileManager
{
    public partial class MainWindow : Window
    {
        private readonly AppPane _leftPane;
        private readonly AppPane _rightPane;
        private readonly ConfigManager _configManager;

        public MainWindow()
        {
            InitializeComponent();

            _configManager = new ConfigManager();

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

                if (Keyboard.IsKeyDown(Key.F))
                {
                    var pane = GetPaneToHandle(sender);
                    Keyboard.Focus(pane.GetGridName() == "LeftPaneData"
                        ? LeftPaneSearch.PaneSearchTextBox
                        : RightPaneSearch.PaneSearchTextBox);
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
    }
}