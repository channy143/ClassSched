using ClassSched.ViewModels;

namespace ClassSched.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _viewModel;

    public SchedulePage(ScheduleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadClassesAsync();
    }

    private async void OnSchedulePageAppearing(object sender, EventArgs e)
    {
        await _viewModel.LoadClassesAsync();
    }
}
