using System.Windows;
using System.Windows.Controls;
using WinAppInstaller.Models;

namespace WinAppInstaller.Views;

public partial class SettingsWindow : Window
{
    private AppSettings _settings;

    public SettingsWindow()
    {
        InitializeComponent();
        _settings = App.SettingsService!.LoadSettings();
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Theme
        ThemeComboBox.SelectedIndex = _settings.Theme switch
        {
            AppTheme.Minimal => 0,
            AppTheme.Fluent => 1,
            AppTheme.Material => 2,
            _ => 0
        };

        // Other settings
        DarkModeCheckBox.IsChecked = _settings.DarkMode;
        CheckUpdatesOnStartupCheckBox.IsChecked = _settings.CheckForUpdatesOnStartup;
        ShowWelcomeCheckBox.IsChecked = _settings.ShowWelcomeScreen;
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Theme switching logic would go here
        // This would require reloading the UI or restarting the app
    }

    private void ManageScheduleButton_Click(object sender, RoutedEventArgs e)
    {
        var scheduleWindow = new ScheduleWindow();
        scheduleWindow.Owner = this;
        scheduleWindow.ShowDialog();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Save theme
        _settings.Theme = ThemeComboBox.SelectedIndex switch
        {
            0 => AppTheme.Minimal,
            1 => AppTheme.Fluent,
            2 => AppTheme.Material,
            _ => AppTheme.Minimal
        };

        // Save other settings
        _settings.DarkMode = DarkModeCheckBox.IsChecked ?? false;
        _settings.CheckForUpdatesOnStartup = CheckUpdatesOnStartupCheckBox.IsChecked ?? true;
        _settings.ShowWelcomeScreen = ShowWelcomeCheckBox.IsChecked ?? true;

        App.SettingsService!.SaveSettings(_settings);

        MessageBox.Show(
            "Settings saved successfully! Some changes may require restarting the app.",
            "Settings Saved",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );

        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
