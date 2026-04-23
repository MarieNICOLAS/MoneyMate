using MoneyMate.ViewModels.ComponentsViewModel;

namespace MoneyMate.Components.Layout;

public partial class Navbar : ContentView
{
    public Navbar()
    {
        InitializeComponent();
        BindingContext = new NavbarViewModel();
    }
}