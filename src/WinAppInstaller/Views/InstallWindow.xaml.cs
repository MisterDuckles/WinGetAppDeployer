using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinAppInstaller.Models;
using AppModel = WinAppInstaller.Models.App;

namespace WinAppInstaller.Views;

public partial class InstallWindow : Window
{
    private readonly List<AppModel> _appsToInstall;
    private readonly Dictionary<string, TextBlock> _appStatusTexts = new();

    public InstallWindow(List<AppModel> appsToInstall)
    {
        InitializeComponent();
        _appsToInstall = appsToInstall;
        Loaded += InstallWindow_Loaded;
    }

    private async void InstallWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Create status text for each app
        foreach (var app in _appsToInstall)
        {
            var appPanel = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stackPanel = new StackPanel();

            var nameText = new TextBlock
            {
                Text = app.Name,
                FontSize = 14,
                FontWeight = FontWeights.Medium
            };

            var statusText = new TextBlock
            {
                Text = "⏳ Waiting...",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                Margin = new Thickness(0, 5, 0, 0)
            };

            _appStatusTexts[app.WingetId] = statusText;

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(statusText);
            appPanel.Child = stackPanel;

            InstallLogPanel.Children.Add(appPanel);
        }

        // Start installation
        await InstallAppsAsync();
    }

    private async Task InstallAppsAsync()
    {
        var progress = new Progress<(int current, int total, string appName, string message)>(update =>
        {
            Dispatcher.Invoke(() =>
            {
                CurrentAppText.Text = $"Installing app {update.current} of {update.total}: {update.appName}";
                OverallProgressBar.Value = (update.current * 100.0) / update.total;

                // Update specific app status
                var app = _appsToInstall.FirstOrDefault(a => a.Name == update.appName);
                if (app != null && _appStatusTexts.TryGetValue(app.WingetId, out var statusText))
                {
                    statusText.Text = update.message;

                    if (update.message.Contains("✓"))
                    {
                        statusText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    }
                    else if (update.message.Contains("✗"))
                    {
                        statusText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                    }
                }
            });
        });

        var results = await App.WingetService!.InstallAppsAsync(_appsToInstall, progress);

        Dispatcher.Invoke(() =>
        {
            var successCount = results.Count(r => r.Value);
            var failCount = results.Count - successCount;

            CurrentAppText.Text = successCount == results.Count
                ? $"✓ All {successCount} apps installed successfully!"
                : $"Installation complete: {successCount} succeeded, {failCount} failed";

            CloseButton.IsEnabled = true;
        });
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
