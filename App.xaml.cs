using ClassSched.Services;

namespace ClassSched;

public partial class App : Application
{
    private readonly AppShell _appShell;

    public App(AppShell appShell, NotificationService notificationService)
    {
        InitializeComponent();
        _appShell = appShell;
        _ = InitializeNotificationsAsync(notificationService);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_appShell);
    }

    private static async Task InitializeNotificationsAsync(NotificationService notificationService)
    {
        await notificationService.InitializeAsync();
    }
}
