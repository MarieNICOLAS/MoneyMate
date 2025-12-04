using MoneyMate.ViewModels.ComponentsViewModel;

namespace MoneyMate.Components;

public partial class HeaderAuthenticated : ContentView
{
	public HeaderAuthenticated()
	{
		InitializeComponent();
        BindingContext = new HeaderViewModel();
    }
}