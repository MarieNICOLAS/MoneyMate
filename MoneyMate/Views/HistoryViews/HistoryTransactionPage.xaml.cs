using MoneyMate.Services;
using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class HistoryTransactionPage : ContentPage
{
    public HistoryTransactionPage()
    {
        InitializeComponent();

        var expenseService = IPlatformApplication.Current.Services.GetService<ExpenseService>();
        var categoryService = IPlatformApplication.Current.Services.GetService<CategoryService>();

        BindingContext = new HistoryTransactionViewModel(expenseService, categoryService);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Recharger les données à chaque fois qu'on revient sur la page
        if (BindingContext is HistoryTransactionViewModel vm)
            await vm.LoadAsync();
    }
}
