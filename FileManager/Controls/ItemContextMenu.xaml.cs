using System.Windows;
using System.Windows.Controls;
using Engine;

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

        var selectedItems = currentMainWindow.GetPaneToHandle(row).GetGrid().SelectedItems;

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
            var pane = currentMainWindow.GetPaneToHandle(row);
            foreach (IFileSystemEntry entry in selectedItems)
            {
                pane.DeleteEntry(entry);
            }

            pane.Refresh();
        }
    }

    private void Cut_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenu contextMenu = (ContextMenu)((MenuItem)sender).Parent;
        DataGridRow row = (DataGridRow)contextMenu.PlacementTarget;
        var currentMainWindow = Application.Current.MainWindow as MainWindow;

        var selectedItems = currentMainWindow.GetPaneToHandle(row).GetGrid().SelectedItems;
        currentMainWindow.ClearFilesToCut();
        foreach (IFileSystemEntry entry in selectedItems)
        {
            currentMainWindow.AddItemToCut(entry);
        }
        currentMainWindow.ClearFilesToCopy();
    }

    private void Copy_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenu contextMenu = (ContextMenu)((MenuItem)sender).Parent;
        DataGridRow row = (DataGridRow)contextMenu.PlacementTarget;
        var currentMainWindow = Application.Current.MainWindow as MainWindow;

        var selectedItems = currentMainWindow.GetPaneToHandle(row).GetGrid().SelectedItems;
        currentMainWindow.ClearFilesToCopy();
        foreach (IFileSystemEntry entry in selectedItems)
        {
            currentMainWindow.AddItemToCopy(entry);
        }
        currentMainWindow.ClearFilesToCut();
    }
}