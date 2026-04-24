using ClassSched.ViewModels;

namespace ClassSched.Views;

public partial class AttendanceHistoryPage : ContentPage
{
    public AttendanceHistoryPage(AttendanceHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AttendanceHistoryViewModel viewModel)
        {
            viewModel.LoadDataCommand.Execute(null);
        }
    }
}
