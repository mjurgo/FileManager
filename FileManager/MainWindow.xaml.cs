using System.Diagnostics;
using Engine;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var leftPaneContent = (new FileService()).ListDir(@"C:\\");
            LeftPaneData.ItemsSource = leftPaneContent;
            var rightPaneContent = (new FileService()).ListDir(@"D:\\");
            RightPaneData.ItemsSource = rightPaneContent;
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenItem(sender);
        }
        
        private void Row_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenItem(sender);
            }
        }

        private void OpenItem(object sender)
        {
            if (sender is DataGridRow clickedRow)
            {
                DataGrid? parentDataGrid = FindParent<DataGrid>(clickedRow);

                if (parentDataGrid == null)
                {
                    throw new Exception("Could not find parent data grid for clicked row");
                }
                
                IFileSystemEntry? item = clickedRow.Item as IFileSystemEntry;
                if (item == null)
                {
                    return;
                }

                if (item.Type == EntryType.Directory)
                {
                    OpenDirectory(item, parentDataGrid);
                }
                else if (item.Type == EntryType.File)
                {
                    OpenFile(item, parentDataGrid);
                }
            }
        }

        private void OpenDirectory(IFileSystemEntry item, DataGrid targetGrid)
        {
            var content = (new FileService()).ListDir(item.Path);
            targetGrid.ItemsSource = content;
        }

        private void OpenFile(IFileSystemEntry item, DataGrid targetGrid)
        {
            throw new NotImplementedException();
        }
        
        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T? parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }
    }
}