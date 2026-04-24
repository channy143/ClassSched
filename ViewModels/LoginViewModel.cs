using System.Windows.Input;
using ClassSched.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClassSched.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError = false;

    [ObservableProperty]
    private bool _isBusy = false;

    public bool IsNotBusy => !IsBusy;

    public string PasswordToggleText => IsPasswordHidden ? "Show" : "Hide";

    public ICommand TogglePasswordCommand { get; }
    public ICommand LoginCommand { get; }
    public ICommand NavigateToCreateAccountCommand { get; }

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;

        TogglePasswordCommand = new RelayCommand(() => IsPasswordHidden = !IsPasswordHidden);
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        NavigateToCreateAccountCommand = new AsyncRelayCommand(NavigateToCreateAccountAsync);
    }

    partial void OnIsPasswordHiddenChanged(bool value)
    {
        OnPropertyChanged(nameof(PasswordToggleText));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    public void OnNavigatedTo()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email";
            HasError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your password";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            var (success, message, user) = await _authService.LoginAsync(Email, Password);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Login successful!", "OK");
                await Shell.Current.GoToAsync("//Schedule");
            }
            else
            {
                ErrorMessage = message;
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToCreateAccountAsync()
    {
        await Shell.Current.GoToAsync("//CreateAccountPage");
    }
}
