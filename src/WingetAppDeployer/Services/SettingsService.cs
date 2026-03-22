using System;
using System.IO;
using Newtonsoft.Json;
using WingetAppDeployer.Models;

namespace WingetAppDeployer.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings? _settings;

    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WingetAppDeployer"
        );

        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "settings.json");
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            else
            {
                _settings = new AppSettings();
                SaveSettings(_settings);
            }
        }
        catch
        {
            _settings = new AppSettings();
        }

        return _settings;
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            _settings = settings;
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    public AppSettings CurrentSettings => _settings ?? LoadSettings();
}
