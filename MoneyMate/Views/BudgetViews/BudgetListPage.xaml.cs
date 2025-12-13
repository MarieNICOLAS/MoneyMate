using MoneyMate.ViewModels;

namespace MoneyMate.Views.BudgetViews;

public partial class BudgetListPage : ContentPage
{
    public BudgetListPage(BudgetViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BudgetViewModel vm)
        {
            await vm.LoadBudgetsAsync();

            // Message de retour après modification/ajout
            if (!string.IsNullOrEmpty(vm.ReturnMessage))
            {
                await DisplayAlert("Information", vm.ReturnMessage, "OK");
                vm.ReturnMessage = null;
            }
        }
    }

    private async void OnBudgetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Models.Budget selected)
        {
            await Shell.Current.GoToAsync(
                $"///EditBudgetPage?budgetId={selected.Id}"
            );

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
