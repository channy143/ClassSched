using ClassSched.Services;
using ClassSched.ViewModels;

namespace ClassSched.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        var authService = Application.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
        if (authService != null)
        {
            BindingContext = new LoginViewModel(authService);
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        if (BindingContext is LoginViewModel vm)
        {
            vm.OnNavigatedTo();
        }
    }
}
