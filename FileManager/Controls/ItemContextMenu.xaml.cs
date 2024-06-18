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
        var contextMenu = (ContextMenu)((MenuItem)sender).Parent;
        var row = (DataGridRow)contextMenu.PlacementTarget;
        var currentMainWindow = Application.Current.MainWindow as MainWindow;
        var pane = currentMainWindow.GetPaneToHandle(row);
        
        ActionHandler.RenameEntryAction(pane, row, currentMainWindow);
    }

    private void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        var contextMenu = (ContextMenu)((MenuItem)sender).Parent;
        var row = (DataGridRow)contextMenu.PlacementTarget;
        var currentMainWindow = Application.Current.MainWindow as MainWindow;

        var selectedItems = currentMainWindow.GetPaneToHandle(row).GetGrid().SelectedItems;
        var pane = currentMainWindow.GetPaneToHandle(row);

        ActionHandler.DeleteEntriesAction(pane, selectedItems);
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