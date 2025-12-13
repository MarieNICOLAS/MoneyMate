using MoneyMate.ViewModels;

namespace MoneyMate.Views.ExpensesViews;

public partial class AddExpensePage : ContentPage
{
    public AddExpensePage(ExpenseViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ExpenseViewModel vm)
        {
            await vm.OnAppearAsync();
        }
    }
}
