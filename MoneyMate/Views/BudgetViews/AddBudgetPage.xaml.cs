using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class AddBudgetPage : ContentPage
{
    public AddBudgetPage(BudgetViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}