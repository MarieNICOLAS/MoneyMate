using MoneyMate.ViewModels.ComponentsViewModel;

namespace MoneyMate.Components;

public partial class Header : ContentView
{
    public Header()
    {
        InitializeComponent();

        // Récupération propre du ViewModel via DI MAUI
        var services = Application.Current?.Handler?.MauiContext?.Services;

        BindingContext = services?.GetService<HeaderViewModel>()
            ?? throw new InvalidOperationException("HeaderViewModel n'est pas enregistré dans le conteneur de services.");
    }
}
