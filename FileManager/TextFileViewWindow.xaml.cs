using System.Collections.ObjectModel;
using System.Windows;

namespace FileManager;

public partial class TextFileViewWindow : Window
{
    public string FileName { get; }
    public ObservableCollection<string> FileContent{ get; }
    public TextFileViewWindow(string fileName, ObservableCollection<string> fileContent)
    {
        FileContent = fileContent;
        
        InitializeComponent();
        DataContext = this;
        
        FileName = fileName;
    }
}