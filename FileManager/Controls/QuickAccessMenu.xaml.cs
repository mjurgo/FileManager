using System.Windows;
using System.Windows.Controls;
using Engine.Config;

namespace FileManager.Controls;

public partial class QuickAccessMenu
{
    private readonly UserPreferencesManager _userPreferencesManager;

    public QuickAccessMenu()
    {
        _userPreferencesManager = new UserPreferencesManager();
        _userPreferencesManager.GetQuickAccessLocations();

        UpdateMenuItems();

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

    private void AddLocation_Click(object sender, RoutedEventArgs e)
    {
        InputDialogWindow inputWindow = new InputDialogWindow("Input full path to new location");
        if (inputWindow.ShowDialog() == true)
        {
            string input = inputWindow.InputText;
            _userPreferencesManager.AddNewQuickAccessLocation(input);
            UpdateMenuItems();
        }
    }

    private void UpdateMenuItems()
    {
        Items.Clear();
        foreach (string location in _userPreferencesManager.GetQuickAccessLocations())
        {
            var item = new MenuItem
            {
                Header = location,
                DataContext = location,
            };
            item.Click += Location_Click;
            Items.Add(item);
        }

        var addNew = new MenuItem
        {
            Header = "+ Add location...",
        };
        addNew.Click += AddLocation_Click;
        Items.Add(addNew);
    }
}