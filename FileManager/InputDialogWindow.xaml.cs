using System.Windows;
using System.Windows.Controls;

namespace FileManager;

public partial class InputDialogWindow : Window
{
    public string InputText { get; private set; }

    public InputDialogWindow(string prompt)
    {
        InitializeComponent();
        Prompt.Content = prompt;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        InputText = InputTextBox.Text;
        DialogResult = true;
    }
}