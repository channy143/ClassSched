using ClassSched.Services;
using ClassSched.ViewModels;

namespace ClassSched.Views;

public partial class CalendarPage : ContentPage
{
    public CalendarPage()
    {
        InitializeComponent();

        var databaseService = Application.Current?.Handler?.MauiContext?.Services.GetService<DatabaseService>();
        
        if (databaseService != null)
        {
            BindingContext = new CalendarViewModel(databaseService);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is CalendarViewModel vm)
        {
            _ = vm.LoadDataAsync();
        }
    }
}
