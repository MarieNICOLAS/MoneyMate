namespace MoneyMate.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
		BindingContext = new ViewModels.HomeViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HomeViewModel vm)
        {
            Task.Run(vm.LoadDashboardDataAsync);
        }
    }
}