using ClassSched.Services;
using ClassSched.ViewModels;

namespace ClassSched.Views;

public partial class CreateAccountPage : ContentPage
{
    public CreateAccountPage()
    {
        InitializeComponent();

        var authService = Application.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
        if (authService != null)
        {
            BindingContext = new CreateAccountViewModel(authService);
        }
    }
}
