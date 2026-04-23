using MoneyMate.ViewModels.ComponentsViewModel;

namespace MoneyMate.Components;

public partial class Navbar : ContentView
{
	public Navbar()
	{
		InitializeComponent();
		BindingContext = new NavbarViewModel();
	}
}