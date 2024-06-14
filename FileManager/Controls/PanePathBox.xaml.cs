using System.Windows.Controls;

namespace FileManager.Controls;

public partial class PanePathBox : UserControl
{
    public PanePathBox()
    {
        InitializeComponent();
    }

    public void SetPath(string path)
    {
        PanePathTextBox.Text = path;
    }
}