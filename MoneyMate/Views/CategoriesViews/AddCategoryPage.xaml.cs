using MoneyMate.ViewModels;

namespace MoneyMate.Views.CategoriesViews;

public partial class AddCategoryPage : ContentPage
{
	public AddCategoryPage(CategoryViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

    }
}