using System.Windows;
using System.Windows.Controls;

namespace FileManager.Controls;

public partial class ItemContextMenu : ContextMenu
{
    public ItemContextMenu()
    {
        InitializeComponent();
    }

    private void Open_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenu contextMenu = (ContextMenu)((MenuItem)sender).Parent;
        DataGridRow row = (DataGridRow)contextMenu.PlacementTarget;
        var currentMainWindow = Application.Current.MainWindow as MainWindow;
        currentMainWindow?.GetPaneToHandle(row).OpenItem(row);
    }

    private void Rename_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenu contextMenu = (ContextMenu)((MenuItem)sender).Parent;
        DataGridRow row = (DataGridRow)contextMenu.PlacementTarget;
        var currentMainWindow = Application.Current.MainWindow as MainWindow;
        
        InputDialogWindow inputWindow = new InputDialogWindow("Enter a new name for the item");
        if (inputWindow.ShowDialog() == true)
        {
            string input = inputWindow.InputText;
            var pane = currentMainWindow.GetPaneToHandle(row);
            pane.RenameEntry(row, input);
            pane.Refresh();
        }
    }

    private void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenu contextMenu = (ContextMenu)((MenuItem)sender).Parent;
        DataGridRow row = (DataGridRow)contextMenu.PlacementTarget;
        var currentMainWindow = Application.Current.MainWindow as MainWindow;

        var confirmed = MessageBox.Show(
            "Are you sure you want to delete this item?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirmed == MessageBoxResult.Yes)
        {
            var pane = currentMainWindow.GetPaneToHandle(row);
            pane.DeleteEntry(row);
            pane.Refresh();
        }
    }
}