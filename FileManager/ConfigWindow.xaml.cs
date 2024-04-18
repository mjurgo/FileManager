using System.Windows;
using System.Windows.Controls;
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
}