using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ClassSched.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SQLite;

namespace ClassSched.Services;

public class AuthService
{
    private readonly DatabaseService _databaseService;
    private const string SmtpHost = "smtp.gmail.com";
    private const int SmtpPort = 587;
    private const string SmtpUsername = "shirochan1106@gmail.com";
    private const string SmtpPassword = "npoc ypee dtyt cwub"; // Replace with actual app password

    public AuthService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            var connection = await _databaseService.GetConnectionAsync();

            // Check if email already exists
            var existingUser = await connection.Table<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (existingUser != null)
            {
                return (false, "Email already registered", null);
            }

            // Generate verification code
            var verificationCode = GenerateVerificationCode();

            // Try sending email FIRST before saving user
            var emailSent = await SendVerificationEmailAsync(email, verificationCode, firstName);
            
            if (!emailSent)
            {
                return (false, "Failed to send verification email. Please check your email address and try again.", null);
            }

            // Only save user if email was sent successfully
            var user = new User
            {
                Email = email.ToLower(),
                PasswordHash = HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                IsEmailVerified = false,
                EmailVerificationCode = verificationCode,
                VerificationCodeExpiry = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            await connection.InsertAsync(user);

            return (true, "Account created. Please check your email for verification code.", user);
        }
        catch (Exception ex)
        {
            return (false, $"Registration failed: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string email, string code)
    {
        try
        {
            var connection = await _databaseService.GetConnectionAsync();
            var user = await connection.Table<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return (false, "User not found");
            }

            if (user.IsEmailVerified)
            {
                return (false, "Email already verified");
            }

            if (user.EmailVerificationCode != code)
            {
                return (false, "Invalid verification code");
            }

            if (user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return (false, "Verification code expired. Please request a new one.");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.VerificationCodeExpiry = null;

            await connection.UpdateAsync(user);

            return (true, "Email verified successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Verification failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
    {
        try
        {
            var connection = await _databaseService.GetConnectionAsync();
            var user = await connection.Table<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return (false, "Invalid email or password", null);
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Invalid email or password", null);
            }

            if (!user.IsEmailVerified)
            {
                return (false, "Please verify your email before logging in", null);
            }

            user.LastLoginAt = DateTime.UtcNow;
            await connection.UpdateAsync(user);

            // Store current user in preferences
            Preferences.Set("CurrentUserId", user.Id);
            Preferences.Set("CurrentUserEmail", user.Email);
            Preferences.Set("IsLoggedIn", true);

            return (true, "Login successful", user);
        }
        catch (Exception ex)
        {
            return (false, $"Login failed: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> ResendVerificationCodeAsync(string email)
    {
        try
        {
            var connection = await _databaseService.GetConnectionAsync();
            var user = await connection.Table<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return (false, "User not found");
            }

            if (user.IsEmailVerified)
            {
                return (false, "Email already verified");
            }

            var newCode = GenerateVerificationCode();
            user.EmailVerificationCode = newCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddHours(24);

            await connection.UpdateAsync(user);
            await SendVerificationEmailAsync(user.Email, newCode, user.FirstName);

            return (true, "New verification code sent");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to resend code: {ex.Message}");
        }
    }

    public void Logout()
    {
        Preferences.Remove("CurrentUserId");
        Preferences.Remove("CurrentUserEmail");
        Preferences.Set("IsLoggedIn", false);
    }

    public bool IsLoggedIn()
    {
        return Preferences.Get("IsLoggedIn", false);
    }

    public User? GetCurrentUser()
    {
        var userId = Preferences.Get("CurrentUserId", 0);
        if (userId == 0) return null;

        // Note: This returns cached info. Use GetCurrentUserAsync for fresh data.
        return new User
        {
            Id = userId,
            Email = Preferences.Get("CurrentUserEmail", string.Empty)
        };
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = Preferences.Get("CurrentUserId", 0);
        if (userId == 0) return null;

        var connection = await _databaseService.GetConnectionAsync();
        return await connection.Table<User>().FirstOrDefaultAsync(u => u.Id == userId);
    }

    private async Task<bool> SendVerificationEmailAsync(string toEmail, string code, string firstName)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ClassSched", SmtpUsername));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Verify your ClassSched account";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #512BD4;'>Welcome to ClassSched!</h2>
                        <p>Hi {firstName},</p>
                        <p>Thank you for creating an account. Please use the following verification code to verify your email address:</p>
                        <div style='background-color: #f0f0f0; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                            {code}
                        </div>
                        <p>This code will expire in 24 hours.</p>
                        <p>If you didn't create an account, please ignore this email.</p>
                        <p>Best regards,<br>The ClassSched Team</p>
                    </body>
                    </html>",
                TextBody = $@"Welcome to ClassSched!

Hi {firstName},

Thank you for creating an account. Please use the following verification code to verify your email address:

Verification Code: {code}

This code will expire in 24 hours.

If you didn't create an account, please ignore this email.

Best regards,
The ClassSched Team"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Disable certificate validation for mobile environments
            client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            
            await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}
