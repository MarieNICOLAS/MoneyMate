using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class MenuPage : ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
        BindingContext = new MenuViewModel();
    }
}
