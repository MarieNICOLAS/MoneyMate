using MoneyMate.ViewModels.ComponentsViewModel;
namespace MoneyMate.Components;

public partial class Header : ContentView
{
	public Header()
	{
		InitializeComponent();
        BindingContext = new HeaderViewModel();
    }
}