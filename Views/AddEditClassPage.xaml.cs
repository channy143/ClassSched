using ClassSched.ViewModels;

namespace ClassSched.Views;

public partial class AddEditClassPage : ContentPage
{
    private readonly AddEditClassViewModel _viewModel;

    public AddEditClassPage(AddEditClassViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _viewModel.LoadClassAsync();
    }
}
