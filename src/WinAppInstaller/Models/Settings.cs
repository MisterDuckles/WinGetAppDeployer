namespace WinAppInstaller.Models;

public enum AppTheme
{
    Minimal,
    Fluent,
    Material
}

public class AppSettings
{
    public AppTheme Theme { get; set; } = AppTheme.Minimal;
    public bool DarkMode { get; set; }
    public bool CheckForUpdatesOnStartup { get; set; } = true;
    public bool ShowWelcomeScreen { get; set; } = true;
    public bool AutoUpdateEnabled { get; set; }
    public string? AutoUpdateSchedule { get; set; }
}
