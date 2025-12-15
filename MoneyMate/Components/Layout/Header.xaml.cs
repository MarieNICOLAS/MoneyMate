using MoneyMate.ViewModels.ComponentsViewModel;
namespace MoneyMate.Components.Layout;

public partial class Header : ContentView
{
	public Header()
	{
		InitializeComponent();
        BindingContext = new HeaderViewModel();
    }
}