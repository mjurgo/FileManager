using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.Config;

public class UserPreferencesManager
{
    private readonly string _jsonContent;
    private readonly JsonElement _preferences;

    public UserPreferencesManager()
    {
        _jsonContent = File.ReadAllText("user_preferences.json");
    }

    public string[] GetQuickAccessLocations()
    {
        using JsonDocument doc = JsonDocument.Parse(_jsonContent);
        var root = doc.RootElement;
        var property = root.GetProperty("QuickAccess");
        return property.EnumerateArray().Select(prop => prop.ToString()).ToArray();
    }
}