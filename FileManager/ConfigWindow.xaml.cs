using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Engine.Config;

namespace FileManager;

public partial class ConfigWindow : Window
{
    private ConfigManager _configManager;
    
    public ConfigWindow()
    {
        InitializeComponent();
        _configManager = new ConfigManager();
        
        var categories = _configManager.GetConfigCategories();
        Categories.ItemsSource = categories;
    }

    private void Categories_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedCategory = (string)Categories.SelectedItem;
        var options = _configManager.GetCategoryOptions(selectedCategory);
        Options.ItemsSource = options;
    }
    private void LightThemeClick(object sender, RoutedEventArgs e)
    {

        AppTheme.ChangeTheme(new Uri("Themes/Light.xaml", UriKind.Relative));

    }

    private void DarkThemeClick(object sender, RoutedEventArgs e)
    {
        AppTheme.ChangeTheme(new Uri("Themes/Dark.xaml", UriKind.Relative));
    }

    private void OptionsSave_Click(object sender, RoutedEventArgs e)
    {
        string? category = Categories.SelectedItem as string;
        List<Option>? options = Options.ItemsSource as List<Option>;
        if (category == null || options == null)
        {
            MessageBox.Show("Cannot save config file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        _configManager.SetCategoryOptions(category, options);
        _configManager.SaveConfig();
    }

    private void TextOption_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var options = Options.ItemsSource;
        if (sender is TextBox clickedOption)
        {
            var parent = FindParent<DataGridRow>(clickedOption);
            string optionName = ((Option)parent.DataContext).Name;
            foreach (Option option in options)
            {
                if (option.Name == optionName)
                {
                    option.Value = (sender as TextBox).Text;
                }
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

    private void SelectOption_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var options = Options.ItemsSource;
        if (sender is ComboBox selectedOption)
        {
            var parent = FindParent<DataGridRow>(selectedOption);
            string optionName = ((Option)parent.DataContext).Name;
            foreach (Option option in options)
            {
                if (option.Name == optionName)
                {
                    option.Value = (sender as ComboBox).SelectedValue.ToString();
                }
            }
        }
    }
}