using MoneyMate.ViewModels.ComponentsViewModel;

namespace MoneyMate.Components;

public partial class HeaderAuthenticated : ContentView
{
    public HeaderAuthenticated()
    {
        InitializeComponent();

        // Récupération du HeaderViewModel via le conteneur de services MAUI
        var services = Application.Current?.Handler?.MauiContext?.Services;

        BindingContext = services?.GetService<HeaderViewModel>()
                        ?? throw new InvalidOperationException("HeaderViewModel n'est pas enregistré dans le conteneur de services.");
    }
}
