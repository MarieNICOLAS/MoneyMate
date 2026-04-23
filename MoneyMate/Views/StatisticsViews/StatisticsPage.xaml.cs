using MoneyMate.Services;
using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class StatisticsPage : ContentPage
{
    public StatisticsPage()
    {
        InitializeComponent();
        var expenseService = IPlatformApplication.Current.Services.GetService<ExpenseService>();
        var budgetService = IPlatformApplication.Current.Services.GetService<BudgetService>();
        BindingContext = new StatisticsViewModel(expenseService, budgetService);
    }
}
