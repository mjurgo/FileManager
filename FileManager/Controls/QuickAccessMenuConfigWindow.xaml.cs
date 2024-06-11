using System.Windows;
using Engine.Config;

namespace FileManager.Controls;

public partial class QuickAccessMenuConfigWindow : Window
{
    private readonly UserPreferencesManager _userPreferencesManager = new();

    public QuickAccessMenuConfigWindow()
    {
        InitializeComponent();

        QuickAccessLocations.ItemsSource = _userPreferencesManager.GetQuickAccessLocations();
    }

    private void AddLocation_OnClick(object sender, RoutedEventArgs e)
    {
        InputDialogWindow inputWindow = new InputDialogWindow("Input full path to new location");
        if (inputWindow.ShowDialog() != true) return;
        string input = inputWindow.InputText;
        _userPreferencesManager.AddNewQuickAccessLocation(input);
        QuickAccessLocations.ItemsSource = _userPreferencesManager.GetQuickAccessLocations();
    }

    private void DeleteLocation_OnClick(object sender, RoutedEventArgs e)
    {
        var location = QuickAccessLocations.SelectedItem.ToString();
        _userPreferencesManager.DeleteQuickAccessLocation(location);
        QuickAccessLocations.ItemsSource = _userPreferencesManager.GetQuickAccessLocations();
    }
}