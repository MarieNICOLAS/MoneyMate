using MoneyMate.Database;
using System.Diagnostics;

namespace MoneyMate
{
    public partial class App : Application
    {
        // ✅ Propriété statique accessible depuis tout le projet
        public static MoneyMateContext Database { get; private set; }

        public App(MoneyMateContext dbContext)
        {
            InitializeComponent();

            // Initialise la base de données pour tout le projet
            Database = dbContext;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            InitializeDatabaseSafe();
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
