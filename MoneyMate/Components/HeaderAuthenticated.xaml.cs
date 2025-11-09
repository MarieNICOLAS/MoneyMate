using MoneyMate.ViewModels;

namespace MoneyMate.Components;

public partial class HeaderAuthenticated : ContentView
{
	public HeaderAuthenticated()
	{
		InitializeComponent();
        BindingContext = new HeaderViewModel();
    }
}