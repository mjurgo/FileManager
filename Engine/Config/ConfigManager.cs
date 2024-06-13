using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.Config;

public class Option
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("value")]
    public string Value { get; set; }
    [JsonPropertyName("type")]
    public OptionType Type { get; set; }
    [JsonPropertyName("possibleValues")]
    public List<string> PossibleValues { get; set; }
}

public class ConfigManager
{
    private Dictionary<string?, List<Option>?> _config;
    public ConfigManager()
    {
        var jsonContent = File.ReadAllText("config.json");
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _config = JsonSerializer.Deserialize<Dictionary<string, List<Option>>>(jsonContent, options) ??
                 throw new FileNotFoundException("Config file not found");
    }

    public List<string?> GetConfigCategories()
    {
        return _config.Keys.ToList();
    }

    public List<Option>? GetCategoryOptions(string? categoryName)
    {
        return _config[categoryName];
    }

    public void SetCategoryOptions(string? categoryName, List<Option>? options)
    {
        _config[categoryName] = options;
    }

    public void SaveConfig()
    {
        string configSerialized = JsonSerializer.Serialize(_config);
        File.WriteAllText("config.json", configSerialized);
    }

    public string GetLeftPaneDefaultLocation()
    {
        return _config["Panes"].Find(option => option.Name == "LeftPaneDefaultLocation").Value;
    }
    
    public string GetRightPaneDefaultLocation()
    {
        return _config["Panes"].Find(option => option.Name == "RightPaneDefaultLocation").Value;
    }

    public string GetCurrentTheme()
    {
        return _config["Appearance"].Find(option => option.Name == "Color").Value;
    }
}