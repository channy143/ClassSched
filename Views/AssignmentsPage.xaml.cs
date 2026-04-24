using ClassSched.Services;
using ClassSched.ViewModels;

namespace ClassSched.Views;

public partial class AssignmentsPage : ContentPage
{
    public AssignmentsPage()
    {
        InitializeComponent();

        var databaseService = Application.Current?.Handler?.MauiContext?.Services.GetService<DatabaseService>();
        var notificationService = Application.Current?.Handler?.MauiContext?.Services.GetService<NotificationService>();
        
        if (databaseService != null && notificationService != null)
        {
            BindingContext = new AssignmentsViewModel(databaseService, notificationService);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is AssignmentsViewModel vm)
        {
            _ = vm.LoadAssignmentsAsync();
        }
    }
}
