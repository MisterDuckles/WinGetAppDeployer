using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WingetAppDeployer.Services;

public class TaskSchedulerService
{
    private const string TaskName = "WingetAppDeployer_AutoUpdate";

    /// <summary>
    /// Creates a scheduled task to check for app updates
    /// </summary>
    public async Task<bool> CreateUpdateTaskAsync(UpdateScheduleType scheduleType, string? customTime = null)
    {
        try
        {
            var exePath = Environment.ProcessPath ?? string.Empty;
            if (string.IsNullOrEmpty(exePath))
                return false;

            // Delete existing task if it exists
            await DeleteUpdateTaskAsync();

            var trigger = scheduleType switch
            {
                UpdateScheduleType.Daily => $"/sc daily /st {customTime ?? "09:00"}",
                UpdateScheduleType.Weekly => $"/sc weekly /d MON /st {customTime ?? "09:00"}",
                UpdateScheduleType.OnStartup => "/sc onlogon",
                _ => "/sc daily /st 09:00"
            };

            var arguments = $"/create /tn \"{TaskName}\" {trigger} /tr \"\\\"{exePath}\\\" /autoupdate\" /f /rl highest";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas" // Request admin
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create scheduled task: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes the auto-update scheduled task
    /// </summary>
    public async Task<bool> DeleteUpdateTaskAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/delete /tn \"{TaskName}\" /f",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            // Exit code 0 = success, 1 = task doesn't exist (also OK)
            return process.ExitCode == 0 || process.ExitCode == 1;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the auto-update task exists
    /// </summary>
    public async Task<bool> TaskExistsAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/query /tn \"{TaskName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Runs the update task immediately
    /// </summary>
    public async Task<bool> RunUpdateTaskNowAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/run /tn \"{TaskName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

public enum UpdateScheduleType
{
    Daily,
    Weekly,
    OnStartup
}
