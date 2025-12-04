using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class AddCategoryPage : ContentPage
{
	public AddCategoryPage(CategoryViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

    }
}