using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClassSched.Services;
using System.Threading.Tasks;

namespace ClassSched.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    private int _reminderMinutes = 15;

    [ObservableProperty]
    private bool _isDarkMode;

    public List<int> ReminderOptions { get; } = new List<int> { 5, 10, 15, 30, 60 };

    public SettingsViewModel(SettingsService settingsService, NotificationService notificationService)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;
    }

    public async Task LoadSettingsAsync()
    {
        NotificationsEnabled = await _settingsService.GetNotificationsEnabledAsync();
        ReminderMinutes = await _settingsService.GetReminderMinutesAsync();
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        await _settingsService.SetNotificationsEnabledAsync(NotificationsEnabled);
        await _settingsService.SetReminderMinutesAsync(ReminderMinutes);

        if (!NotificationsEnabled)
        {
            _notificationService.CancelAllNotifications();
        }

        await Shell.Current.DisplayAlert("Settings Saved", "Your settings have been saved successfully.", "OK");
    }

    [RelayCommand]
    private async Task TestNotificationAsync()
    {
        await _notificationService.SendTestNotificationAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("//Schedule");
    }

    partial void OnNotificationsEnabledChanged(bool value)
    {
        _ = SaveSettingsAsync();
    }

    partial void OnReminderMinutesChanged(int value)
    {
        _ = SaveSettingsAsync();
    }
}
