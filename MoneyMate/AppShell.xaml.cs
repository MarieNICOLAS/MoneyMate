using MoneyMate.Services;
using Microsoft.Maui.Controls;

namespace MoneyMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            CheckLoginStatus();
        }

        /// <summary>
        /// Vérifie l'état de la connexion et redirige l'utilisateur.
        /// </summary>
        private async void CheckLoginStatus()
        {
            var authService = new AuthService(App.Database);
            var user = await authService.GetLoggedInUserAsync();

            if (user != null)
            {
                await Shell.Current.GoToAsync("//DashboardPage");
            }
        }
    }
}
