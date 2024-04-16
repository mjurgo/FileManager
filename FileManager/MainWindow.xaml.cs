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
        private readonly IFileService _fileService;

        private AppPane _leftPane;
        private AppPane _rightPane;

        public MainWindow()
        {
            InitializeComponent();

            _leftPane = new AppPane(@"C:\\", LeftPaneData);
            _rightPane = new AppPane(@"D:\\", RightPaneData);

            LeftPaneData.ItemsSource = _leftPane.Content;
            RightPaneData.ItemsSource = _rightPane.Content;
            
            _fileService = new FileService();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            GetPaneToHandle(sender).OpenItem(sender);
        }
        
        private void Row_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Keyboard.IsKeyDown((Key.Right)))
                {
                    Debug.WriteLine("KEY RIGHT DETECTED");
                    GetPaneToHandle(sender).GoDirForward();
                }
                if (Keyboard.IsKeyDown((Key.Left)))
                {
                    Debug.WriteLine("KEY LEFT DETECTED");
                    GetPaneToHandle(sender).GoDirBack();
                }
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    GetPaneToHandle(sender).OpenItem(sender);
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
    }
}