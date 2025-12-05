using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class EditBudgetPage : ContentPage
{
    public EditBudgetPage(BudgetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BudgetViewModel vm)
        {
            // Si on veut recharger l’historique des budgets
            await vm.LoadBudgetsAsync();
        }
    }
}
