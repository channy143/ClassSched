namespace ClassSched.Services;

public class SettingsService
{
    private const string NotificationsEnabledKey = "NotificationsEnabled";
    private const string ReminderMinutesKey = "ReminderMinutes";

    public async Task<bool> GetNotificationsEnabledAsync()
    {
        return await GetPreferenceAsync(NotificationsEnabledKey, true);
    }

    public async Task SetNotificationsEnabledAsync(bool enabled)
    {
        await SetPreferenceAsync(NotificationsEnabledKey, enabled);
    }

    public async Task<int> GetReminderMinutesAsync()
    {
        return await GetPreferenceAsync(ReminderMinutesKey, 15);
    }

    public async Task SetReminderMinutesAsync(int minutes)
    {
        await SetPreferenceAsync(ReminderMinutesKey, minutes);
    }

    private async Task<T> GetPreferenceAsync<T>(string key, T defaultValue)
    {
        await Task.CompletedTask;
        return Preferences.Default.Get(key, defaultValue);
    }

    private async Task SetPreferenceAsync<T>(string key, T value)
    {
        await Task.CompletedTask;
        Preferences.Default.Set(key, value);
    }
}
