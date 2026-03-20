using System.Threading.Tasks;
using System.Windows;
using WinAppInstaller.Services;

namespace WinAppInstaller;

public partial class App : Application
{
    public static GitHubService? GitHubService { get; private set; }
    public static WingetService? WingetService { get; private set; }
    public static SettingsService? SettingsService { get; private set; }
    public static TaskSchedulerService? TaskSchedulerService { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize services
        GitHubService = new GitHubService();
        WingetService = new WingetService();
        SettingsService = new SettingsService();
        TaskSchedulerService = new TaskSchedulerService();

        // Check for command line arguments
        if (e.Args.Length > 0)
        {
            if (e.Args[0] == "/autoupdate")
            {
                // Run auto-update in background
                Task.Run(async () =>
                {
                    await WingetService.UpdateAllApps();
                });
                Shutdown();
                return;
            }
        }
    }
}
