using System.Windows.Input;
using ClassSched.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClassSched.ViewModels;

public partial class CreateAccountViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _verificationCode = string.Empty;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private bool _isConfirmPasswordHidden = true;

    [ObservableProperty]
    private bool _isRegistrationStep = true;

    [ObservableProperty]
    private bool _isVerificationStep = false;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _hasMessage = false;

    [ObservableProperty]
    private bool _isError = false;

    [ObservableProperty]
    private bool _isBusy = false;

    public bool IsNotBusy => !IsBusy;

    public string PasswordToggleText => IsPasswordHidden ? "Show" : "Hide";

    public string VerificationMessage => $"We've sent a verification code to {Email}. Please enter the code below to verify your account.";

    public ICommand TogglePasswordCommand { get; }
    public ICommand CreateAccountCommand { get; }
    public ICommand VerifyEmailCommand { get; }
    public ICommand ResendCodeCommand { get; }
    public ICommand NavigateToLoginCommand { get; }

    public CreateAccountViewModel(AuthService authService)
    {
        _authService = authService;

        TogglePasswordCommand = new RelayCommand(() => IsPasswordHidden = !IsPasswordHidden);
        CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync);
        VerifyEmailCommand = new AsyncRelayCommand(VerifyEmailAsync);
        ResendCodeCommand = new AsyncRelayCommand(ResendCodeAsync);
        NavigateToLoginCommand = new AsyncRelayCommand(NavigateToLoginAsync);
    }

    partial void OnIsPasswordHiddenChanged(bool value)
    {
        OnPropertyChanged(nameof(PasswordToggleText));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    private async Task CreateAccountAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ShowError("Please enter your first name");
            return;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            ShowError("Please enter your last name");
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("Please enter your email");
            return;
        }

        if (!IsValidEmail(Email))
        {
            ShowError("Please enter a valid email address");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Please enter a password");
            return;
        }

        if (Password.Length < 6)
        {
            ShowError("Password must be at least 6 characters long");
            return;
        }

        if (Password != ConfirmPassword)
        {
            ShowError("Passwords do not match");
            return;
        }

        IsBusy = true;
        HasMessage = false;

        try
        {
            var (success, message, user) = await _authService.RegisterAsync(Email, Password, FirstName, LastName);

            if (success)
            {
                IsRegistrationStep = false;
                IsVerificationStep = true;
                ShowSuccess(message);
            }
            else
            {
                ShowError(message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Registration failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task VerifyEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(VerificationCode) || VerificationCode.Length != 6)
        {
            ShowError("Please enter the 6-digit verification code");
            return;
        }

        IsBusy = true;
        HasMessage = false;

        try
        {
            var (success, message) = await _authService.VerifyEmailAsync(Email, VerificationCode);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Email verified! You can now log in.", "OK");
                await Shell.Current.GoToAsync("//Login");
            }
            else
            {
                ShowError(message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Verification failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResendCodeAsync()
    {
        IsBusy = true;
        HasMessage = false;

        try
        {
            var (success, message) = await _authService.ResendVerificationCodeAsync(Email);

            if (success)
            {
                ShowSuccess(message);
            }
            else
            {
                ShowError(message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to resend code: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("//Login");
    }

    private void ShowError(string error)
    {
        Message = error;
        IsError = true;
        HasMessage = true;
    }

    private void ShowSuccess(string successMessage)
    {
        Message = successMessage;
        IsError = false;
        HasMessage = true;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
