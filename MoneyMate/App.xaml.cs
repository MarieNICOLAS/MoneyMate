using MoneyMate.Database;
using System.Diagnostics;
using MoneyMate.Services;
using Microsoft.Maui.Controls;

namespace MoneyMate
{
    public partial class App : Application
    {
        public static MoneyMateContext Database { get; private set; }

        public App(MoneyMateContext dbContext)
        {
            InitializeComponent();
            Database = dbContext;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            InitializeDatabaseSafe();

            // ✅ Navigation différée une fois la fenêtre prête
            window.Created += async (s, e) =>
            {
                await Task.Delay(500); // petit délai pour s’assurer que Shell est prêt

                if (AuthService.IsUserLoggedIn())
                    await Shell.Current.GoToAsync("//DashboardPage");
                else
                    await Shell.Current.GoToAsync("//MainPage");
            };

            return window;
        }

        private async void InitializeDatabaseSafe()
        {
            try
            {
                await Database.InitializeAsync();
                Debug.WriteLine("✅ Base de données initialisée avec succès !");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Erreur d'initialisation de la base : {ex}");
            }
        }


    }
}
