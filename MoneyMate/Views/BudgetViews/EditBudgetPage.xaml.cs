using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class EditBudgetPage : ContentPage
{
    public EditBudgetPage(BudgetViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}