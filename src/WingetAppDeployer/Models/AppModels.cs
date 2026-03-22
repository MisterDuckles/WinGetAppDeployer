using System.Collections.Generic;

namespace WingetAppDeployer.Models;

public class AppDatabase
{
    public string Version { get; set; } = string.Empty;
    public string LastUpdated { get; set; } = string.Empty;
    public List<Category> Categories { get; set; } = new();
}

public class Category
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<App>? Apps { get; set; }
    public List<SubCategory>? Subcategories { get; set; }
}

public class SubCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<App> Apps { get; set; } = new();
}

public class App
{
    public string Name { get; set; } = string.Empty;
    public string WingetId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Popular { get; set; }
    public bool IsSelected { get; set; }
    public InstallStatus Status { get; set; } = InstallStatus.Unknown;
}

public enum InstallStatus
{
    Unknown,
    NotInstalled,
    Installed,
    Installing,
    UpdateAvailable,
    Failed
}
