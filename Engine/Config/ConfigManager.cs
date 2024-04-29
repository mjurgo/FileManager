using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.Config;

public struct Option
{
    public string Name { get; set; }
    public string Value { get; set; }
    public OptionType Type { get; set; }
    public List<string> PossibleValues { get; set; }
}

public class ConfigManager
{
    private Dictionary<string, List<Option>> _config;
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

    public List<string> GetConfigCategories()
    {
        return _config.Keys.ToList();
    }

    public List<Option> GetCategoryOptions(string categoryName)
    {
        return _config[categoryName];
    }
}