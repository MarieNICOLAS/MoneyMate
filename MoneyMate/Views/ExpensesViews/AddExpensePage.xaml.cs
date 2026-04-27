using MoneyMate.Services;
using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class AddExpensePage : ContentPage
{
    public AddExpensePage()
    {
        InitializeComponent();

        var expenseService = IPlatformApplication.Current.Services.GetService<ExpenseService>();
        var budgetService = IPlatformApplication.Current.Services.GetService<BudgetService>();
        var categoryService = IPlatformApplication.Current.Services.GetService<CategoryService>();

        BindingContext = new ExpenseViewModel(expenseService, budgetService, categoryService);
    }
}
