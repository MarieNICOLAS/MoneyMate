using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class AddBudgetPage : ContentPage
{
    public AddBudgetPage(BudgetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BudgetViewModel vm)
        {
            // Si un jour on veut recharger une liste ici
            await vm.LoadBudgetsAsync();
        }
    }
}
