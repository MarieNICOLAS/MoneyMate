using Microsoft.Maui.Controls;
using MoneyMate.ViewModels;
namespace MoneyMate.Components;

public partial class Header : ContentView
{
	public Header()
	{
		InitializeComponent();
        BindingContext = new HeaderViewModel();
    }
}