using System.Windows;

namespace FileManager;

public partial class TextFileViewWindow : Window
{
    public TextFileViewWindow(string fileName)
    {
        InitializeComponent();

        Title = fileName;
    }
}