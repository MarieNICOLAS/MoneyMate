using MoneyMate.Services;
using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class StatisticsPage : ContentPage
{
    public StatisticsPage(ExpenseService expenseService, BudgetService budgetService)
    {
        InitializeComponent();
        BindingContext = new StatisticsViewModel(expenseService, budgetService);
    }
}