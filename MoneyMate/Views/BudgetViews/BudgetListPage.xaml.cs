using MoneyMate.ViewModels;

namespace MoneyMate.Views.BudgetViews;

public partial class BudgetListPage : ContentPage
{
    public BudgetListPage()
    {
        InitializeComponent();
        BindingContext = new BudgetViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BudgetViewModel vm)
            await vm.LoadBudgetsAsync();
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