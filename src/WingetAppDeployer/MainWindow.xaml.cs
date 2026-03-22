using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WingetAppDeployer.Models;
using WingetAppDeployer.Services;
using WingetAppDeployer.Views;
using AppModel = WingetAppDeployer.Models.App;

namespace WingetAppDeployer;

public partial class MainWindow : Window
{
    private AppDatabase? _appDatabase;
    private readonly List<AppModel> _allApps = new();
    private readonly List<CheckBox> _appCheckBoxes = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Show welcome banner on first run
        var settings = App.SettingsService?.LoadSettings();
        if (settings?.ShowWelcomeScreen == true)
        {
            WelcomeBanner.Visibility = Visibility.Visible;
        }

        // Check if winget is available
        var wingetAvailable = await App.WingetService!.IsWingetAvailableAsync();
        if (!wingetAvailable)
        {
            MessageBox.Show(
                "Winget is not installed or not available. Please install Windows App Installer from the Microsoft Store.",
                "Winget Not Found",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Close();
            return;
        }

        // Check for app updates
        if (settings?.CheckForUpdatesOnStartup == true)
        {
            await CheckForAppUpdatesAsync();
        }

        // Load app database
        await LoadAppDatabaseAsync();
    }

    private async Task CheckForAppUpdatesAsync()
    {
        try
        {
            var (updateAvailable, latestVersion, downloadUrl) = await App.GitHubService!.CheckForUpdatesAsync();

            if (updateAvailable && !string.IsNullOrEmpty(downloadUrl))
            {
                var result = MessageBox.Show(
                    $"A new version ({latestVersion}) is available! Would you like to download and install it?",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information
                );

                if (result == MessageBoxResult.Yes)
                {
                    var progress = new Progress<int>(percent =>
                    {
                        // Could show progress dialog here
                    });

                    await App.GitHubService.DownloadAndInstallUpdateAsync(downloadUrl, progress);
                    Application.Current.Shutdown();
                }
            }
        }
        catch
        {
            // Silently fail update check
        }
    }

    private async Task LoadAppDatabaseAsync()
    {
        try
        {
            LoadingPanel.Visibility = Visibility.Visible;
            AppsPanel.Visibility = Visibility.Collapsed;

            _appDatabase = await App.GitHubService!.DownloadAppDatabaseAsync();

            if (_appDatabase == null)
            {
                MessageBox.Show(
                    "Failed to load app database. Please check your internet connection.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            LoadingPanel.Visibility = Visibility.Collapsed;
            AppsPanel.Visibility = Visibility.Visible;

            RenderCategories();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading apps: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RenderCategories()
    {
        if (_appDatabase == null) return;

        AppsPanel.Children.Clear();
        _allApps.Clear();
        _appCheckBoxes.Clear();

        foreach (var category in _appDatabase.Categories)
        {
            // Category header
            var categoryPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };

            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            headerPanel.Children.Add(new TextBlock
            {
                Text = $"{category.Icon} {category.Name}",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });

            var selectAllButton = new Button
            {
                Content = "Select All",
                Margin = new Thickness(15, 0, 0, 0),
                Padding = new Thickness(10, 5, 10, 5),
                Tag = category
            };
            selectAllButton.Click += SelectAllCategory_Click;
            headerPanel.Children.Add(selectAllButton);

            categoryPanel.Children.Add(headerPanel);

            // Apps in category
            if (category.Apps != null && category.Apps.Any())
            {
                RenderAppList(categoryPanel, category.Apps);
            }

            // Subcategories
            if (category.Subcategories != null)
            {
                foreach (var subcat in category.Subcategories)
                {
                    var subcatHeader = new TextBlock
                    {
                        Text = $"  💻 {subcat.Name}",
                        FontSize = 16,
                        FontWeight = FontWeights.Medium,
                        Margin = new Thickness(0, 10, 0, 5)
                    };
                    categoryPanel.Children.Add(subcatHeader);

                    RenderAppList(categoryPanel, subcat.Apps, true);
                }
            }

            AppsPanel.Children.Add(categoryPanel);
        }
    }

    private void RenderAppList(StackPanel parent, List<AppModel> apps, bool isSubcategory = false)
    {
        var appsGrid = new WrapPanel
        {
            Margin = new Thickness(isSubcategory ? 20 : 0, 0, 0, 0)
        };

        foreach (var app in apps)
        {
            _allApps.Add(app);

            var appCard = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 10, 10),
                Width = 300
            };

            var appPanel = new StackPanel();

            var checkbox = new CheckBox
            {
                Tag = app,
                FontSize = 14,
                FontWeight = FontWeights.Medium
            };
            checkbox.Content = app.Name;
            checkbox.Checked += AppCheckBox_Changed;
            checkbox.Unchecked += AppCheckBox_Changed;

            _appCheckBoxes.Add(checkbox);

            var description = new TextBlock
            {
                Text = app.Description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 0)
            };

            appPanel.Children.Add(checkbox);
            appPanel.Children.Add(description);

            if (app.Popular)
            {
                var popularBadge = new TextBlock
                {
                    Text = "⭐ Popular",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    Margin = new Thickness(0, 5, 0, 0)
                };
                appPanel.Children.Add(popularBadge);
            }

            appCard.Child = appPanel;
            appsGrid.Children.Add(appCard);
        }

        parent.Children.Add(appsGrid);
    }

    private void SelectAllCategory_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var category = button?.Tag as Category;
        if (category == null) return;

        var appsToSelect = new List<AppModel>();

        if (category.Apps != null)
            appsToSelect.AddRange(category.Apps);

        if (category.Subcategories != null)
        {
            foreach (var subcat in category.Subcategories)
                appsToSelect.AddRange(subcat.Apps);
        }

        foreach (var checkbox in _appCheckBoxes)
        {
            if (checkbox.Tag is AppModel app && appsToSelect.Contains(app))
            {
                checkbox.IsChecked = true;
            }
        }
    }

    private void AppCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateSelectionCount();
    }

    private void UpdateSelectionCount()
    {
        var selectedCount = _appCheckBoxes.Count(cb => cb.IsChecked == true);
        SelectionCountText.Text = $"{selectedCount} app{(selectedCount != 1 ? "s" : "")} selected";
        InstallButton.IsEnabled = selectedCount > 0;
    }

    private async void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedApps = _appCheckBoxes
            .Where(cb => cb.IsChecked == true)
            .Select(cb => cb.Tag as AppModel)
            .Where(app => app != null)
            .Cast<AppModel>()
            .ToList();

        if (!selectedApps.Any()) return;

        // Show installation window
        var installWindow = new InstallWindow(selectedApps);
        installWindow.Owner = this;
        installWindow.ShowDialog();

        // Optionally prompt for auto-update setup
        if (App.SettingsService?.CurrentSettings.AutoUpdateEnabled != true)
        {
            var result = MessageBox.Show(
                "Would you like to set up automatic updates for your installed apps?",
                "Auto-Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var scheduleWindow = new ScheduleWindow();
                scheduleWindow.Owner = this;
                scheduleWindow.ShowDialog();
            }
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower();

        foreach (var checkbox in _appCheckBoxes)
        {
            if (checkbox.Tag is AppModel app)
            {
                var parent = checkbox.Parent as StackPanel;
                var card = parent?.Parent as Border;

                if (card != null)
                {
                    card.Visibility = string.IsNullOrWhiteSpace(searchText) ||
                                     app.Name.ToLower().Contains(searchText) ||
                                     app.Description.ToLower().Contains(searchText)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void CloseWelcome_Click(object sender, RoutedEventArgs e)
    {
        WelcomeBanner.Visibility = Visibility.Collapsed;

        var settings = App.SettingsService?.CurrentSettings;
        if (settings != null)
        {
            settings.ShowWelcomeScreen = false;
            App.SettingsService?.SaveSettings(settings);
        }
    }
}
