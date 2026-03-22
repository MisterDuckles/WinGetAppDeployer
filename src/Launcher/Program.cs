using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WingetAppDeployer.Launcher;

class Program
{
    private const string RepoOwner = "MisterDuckles";
    private const string RepoName = "WinGetAppDeployer";
    private static readonly string InstallPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WingetAppDeployer"
    );

    [STAThread]
    static async Task Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show loading message
            await ShowMessageAsync("Winget App Deployer", "Checking for the latest version...");

            // Ensure install directory exists
            Directory.CreateDirectory(InstallPath);

            // Check if app is already downloaded
            var appPath = Path.Combine(InstallPath, "WingetAppDeployer.exe");
            var versionFilePath = Path.Combine(InstallPath, "version.txt");

            string? currentVersion = null;
            if (File.Exists(versionFilePath))
            {
                currentVersion = File.ReadAllText(versionFilePath).Trim();
            }

            // Check for latest version on GitHub
            var (latestVersion, downloadUrl) = await GetLatestReleaseAsync();

            if (string.IsNullOrEmpty(downloadUrl))
            {
                MessageBox.Show(
                    "Could not fetch the latest version from GitHub. Please check your internet connection.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Download if needed
            if (!File.Exists(appPath) || currentVersion != latestVersion)
            {
                await ShowMessageAsync("Winget App Deployer", $"Downloading version {latestVersion}...");

                var success = await DownloadFileAsync(downloadUrl, appPath);

                if (success)
                {
                    File.WriteAllText(versionFilePath, latestVersion);
                }
                else
                {
                    MessageBox.Show(
                        "Failed to download the application. Please try again later.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }
            }

            // Launch the main app
            var startInfo = new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true,
                WorkingDirectory = InstallPath
            };

            // Pass any command line arguments to the main app
            if (args.Length > 0)
            {
                startInfo.Arguments = string.Join(" ", args);
            }

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private static async Task<(string? version, string? downloadUrl)> GetLatestReleaseAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WingetAppDeployer-Launcher");

            var url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var response = await client.GetStringAsync(url);
            var json = JsonNode.Parse(response);

            if (json == null) return (null, null);

            var version = json["tag_name"]?.ToString().TrimStart('v');
            var assets = json["assets"]?.AsArray();

            if (assets == null) return (null, null);

            // Find the main exe asset
            foreach (var asset in assets)
            {
                var name = asset?["name"]?.ToString();
                if (name != null && name.Equals("WingetAppDeployer.exe", StringComparison.OrdinalIgnoreCase))
                {
                    var downloadUrl = asset["browser_download_url"]?.ToString();
                    return (version, downloadUrl);
                }
            }

            return (null, null);
        }
        catch
        {
            return (null, null);
        }
    }

    private static async Task<bool> DownloadFileAsync(string url, string destinationPath)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            await contentStream.CopyToAsync(fileStream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Task ShowMessageAsync(string title, string message)
    {
        return Task.Run(() =>
        {
            // This is a simple message - in a real app you might want a progress dialog
            Console.WriteLine($"{title}: {message}");
        });
    }
}
