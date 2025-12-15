using MoneyMate.ViewModels.ComponentsViewModel;

namespace MoneyMate.Components.Layout;

public partial class HeaderAuthenticated : ContentView
{
	public HeaderAuthenticated()
	{
		InitializeComponent();
        BindingContext = new HeaderViewModel();
    }
}