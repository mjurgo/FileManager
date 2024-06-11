using System.Text.Json;

namespace Engine.Config;

public class UserPreferencesManager
{
    private const string PreferencesFilepath = "user_preferences.json";

    public string[] GetQuickAccessLocations()
    {
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(PreferencesFilepath));
        var root = doc.RootElement;
        var property = root.GetProperty("QuickAccess");
        return property.EnumerateArray().Select(prop => prop.ToString()).ToArray();
    }

    public void AddNewQuickAccessLocation(string location)
    {
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(PreferencesFilepath));
        var quickAccessList = doc.RootElement.GetProperty("QuickAccess").EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToList();

        quickAccessList.Add(location);

        SavePreferences(quickAccessList, doc);
    }

    public void DeleteQuickAccessLocation(string location)
    {
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(PreferencesFilepath));
        var quickAccessList = doc.RootElement.GetProperty("QuickAccess").EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToList();

        quickAccessList.Remove(location);

        SavePreferences(quickAccessList, doc);
    }

    private void SavePreferences(List<string> quickAccessList, JsonDocument doc)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();

            writer.WritePropertyName("QuickAccess");
            writer.WriteStartArray();
            foreach (var item in quickAccessList)
            {
                writer.WriteStringValue(item);
            }

            writer.WriteEndArray();

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                if (property.Name != "QuickAccess")
                {
                    property.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        byte[] jsonBytes = stream.ToArray();
        File.WriteAllBytes("user_preferences.json", jsonBytes);
    }
}