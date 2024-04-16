using System.Windows;

namespace FileManager;

public partial class InputDialogWindow : Window
{
    public string InputText { get; private set; }

    public InputDialogWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        InputText = InputTextBox.Text;
        DialogResult = true;
    }
}