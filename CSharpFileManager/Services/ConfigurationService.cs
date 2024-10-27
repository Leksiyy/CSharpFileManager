using System.Text.Json;
using CSharpFileManager.Models;

namespace CSharpFileManager.Services;
public static class ConfigurationService
{
    public static Config SetConfig()
    {
        string settingsPath = "userSettings.json";
        Config config = LoadConfig(settingsPath);

        Console.WriteLine("Type home directory or press Enter for default directory: ");
        string directory = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = config.HomeDirectory ?? CreateDefaultDirectory();
        }
        else if (!Directory.Exists(directory))
        {
            Console.Clear();
            Console.WriteLine("Directory does not exist.");
        }

        config.HomeDirectory = directory;
        SaveConfig(settingsPath, config);
        Console.WriteLine($"Home directory updated to: {directory}");
        Console.WriteLine($"Current home directory: {config.HomeDirectory}");
        return config;
    }
    
    static Config LoadConfig(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                string jsonString = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Config>(jsonString) ?? new Config();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
        }
        
        return new Config();
    }

    static void SaveConfig(string path, Config config)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }

    static string CreateDefaultDirectory()
    {
        string defaultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "FileManager");
        Directory.CreateDirectory(defaultDirectory);
        Console.WriteLine($"Default directory created at: {defaultDirectory}");
        return defaultDirectory;
    }
}