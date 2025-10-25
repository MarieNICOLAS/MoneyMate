using MoneyMate.Database;
using System.Diagnostics;

namespace MoneyMate
{
    public partial class App : Application
    {
        private readonly MoneyMateContext _dbContext;

        public App(MoneyMateContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // on crée la fenêtre avant d'initialiser la base
            var window = new Window(new AppShell());
            InitializeDatabaseSafe();
            return window;
        }

        private async void InitializeDatabaseSafe()
        {
            try
            {
                await _dbContext.InitializeAsync();
                Debug.WriteLine("✅ Base de données initialisée avec succès !");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Erreur d'initialisation de la base : {ex}");
            }
        }
    }
}
