using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class HistoryExpensePage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryExpensePage(HistoryViewModel viewModel)
	{
		InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HistoryViewModel vm)
        {
            vm.LoadCategoriesCommand.Execute(null); // charge le Picker
            vm.LoadExpensesCommand.Execute(null);   // charge les dépenses
        }
    }
}