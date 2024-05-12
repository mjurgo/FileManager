using System.Diagnostics;
using Engine;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Engine.Config;

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
            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (Keyboard.IsKeyDown(Key.N))
                {
                    InputDialogWindow inputWindow = new InputDialogWindow("Enter the name for new directory");
                    if (inputWindow.ShowDialog() == true)
                    {
                        string input = inputWindow.InputText;
                        var pane = GetPaneToHandle(sender);
                        pane.CreateDirectory(input);
                        pane.Refresh();
                    }
                }
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Keyboard.IsKeyDown((Key.Right)))
                {
                    GetPaneToHandle(sender).GoDirForward();
                }
                if (Keyboard.IsKeyDown((Key.Left)))
                {
                    GetPaneToHandle(sender).GoDirBack();
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
                    var confirmed = MessageBox.Show(
                        "Are you sure you want to delete this item?",
                        "Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (confirmed == MessageBoxResult.Yes)
                    {
                        var pane = GetPaneToHandle(sender);
                        pane.DeleteEntry(sender);
                        pane.Refresh();
                    }
                }
                else if (e.Key == Key.F2)
                {
                    InputDialogWindow inputWindow = new InputDialogWindow("Enter a new name for the item");
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
        
        private AppPane GetPaneToHandle(object sender)
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
    }
}