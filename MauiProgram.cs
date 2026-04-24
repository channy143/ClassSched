using ClassSched.Services;
using ClassSched.ViewModels;
using ClassSched.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using SQLitePCL;

namespace ClassSched;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Initialize SQLite
        Batteries.Init();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<AuthService>();

        // Register ViewModels
        builder.Services.AddSingleton<ScheduleViewModel>();
        builder.Services.AddTransient<AddEditClassViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CreateAccountViewModel>();
        builder.Services.AddTransient<AssignmentsViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();

        // Register Views
        builder.Services.AddSingleton<SchedulePage>();
        builder.Services.AddTransient<AddEditClassPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CreateAccountPage>();
        builder.Services.AddTransient<AssignmentsPage>();
        builder.Services.AddTransient<CalendarPage>();

        // Register AppShell
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
