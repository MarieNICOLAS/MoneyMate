using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class AddExpensePage : ContentPage
{
	public AddExpensePage(ExpenseViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}