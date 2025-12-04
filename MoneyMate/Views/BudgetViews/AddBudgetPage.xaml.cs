namespace MoneyMate.Views;
using MoneyMate.Services;
using MoneyMate.ViewModels;

public partial class AddBudgetPage : ContentPage
{
	public AddBudgetPage(BudgetViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

    }
}