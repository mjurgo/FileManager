using System.Windows;
using System.Windows.Controls;
using Engine.Config;

namespace FileManager.Controls;

public partial class QuickAccessMenu
{
    private UserPreferencesManager _userPreferencesManager;
    
    public QuickAccessMenu()
    {
        _userPreferencesManager = new UserPreferencesManager();
        _userPreferencesManager.GetQuickAccessLocations();

        ItemsSource = _userPreferencesManager.GetQuickAccessLocations();
        
        InitializeComponent();
    }

    private void Location_Click(object sender, RoutedEventArgs e)
    {
        var location = (sender as MenuItem)?.DataContext.ToString();
        if (location == null)
        {
            return;
        }
        var currentMainWindow = Application.Current.MainWindow as MainWindow;
        var pane = ((DataGrid)PlacementTarget).Name switch
        {
            "LeftPaneData" => currentMainWindow?.GetLeftPane(),
            "RightPaneData" => currentMainWindow?.GetRightPane(),
            _ => throw new ArgumentOutOfRangeException()
        };
        pane?.OpenPath(location);
    }
}