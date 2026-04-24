using ClassSched.Services;

namespace ClassSched;

public partial class AppShell : Shell
{
    private readonly AuthService _authService;

    public AppShell(AuthService authService)
    {
        InitializeComponent();
        
        _authService = authService;

        // Register routes
        Routing.RegisterRoute("Login", typeof(Views.LoginPage));
        Routing.RegisterRoute("CreateAccountPage", typeof(Views.CreateAccountPage));
        Routing.RegisterRoute("//Schedule", typeof(Views.SchedulePage));
        Routing.RegisterRoute("//SchedulePage", typeof(Views.SchedulePage));
        Routing.RegisterRoute("//AddEditClass", typeof(Views.AddEditClassPage));
        Routing.RegisterRoute("//Settings", typeof(Views.SettingsPage));
        Routing.RegisterRoute("//Assignments", typeof(Views.AssignmentsPage));
        Routing.RegisterRoute("Calendar", typeof(Views.CalendarPage));
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        // Check authentication state and redirect if needed
        var currentLocation = Shell.Current.CurrentState.Location?.ToString() ?? "";
        
        // Skip auth check for login and create account pages
        if (currentLocation.Contains("Login") || currentLocation.Contains("CreateAccount"))
        {
            return;
        }

        // Check if user is logged in
        if (!_authService.IsLoggedIn())
        {
            // Redirect to login page
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//Login");
            });
        }
    }

    public void NavigateToMainApp()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("//Schedule");
        });
    }

    public void NavigateToLogin()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("//Login");
        });
    }
}
