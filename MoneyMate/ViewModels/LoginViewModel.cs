using System.Windows.Input;
using MoneyMate.Services;
using Microsoft.Maui.Controls;
using MoneyMate.Models;

namespace MoneyMate.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        // Champs liés au formulaire
        private string email;
        private string password;
        private string errorMessage;

        // Propriétés liées à la vue
        public string Email
        {
            get => email;
            set
            {
                if (email != value)
                {
                    email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commande liée au bouton "Se connecter"
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService(App.Database);
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez remplir tous les champs.";
                IsBusy = false;
                return;
            }

            var user = await _authService.LoginAsync(Email, Password);

            if (user == null)
            {
                ErrorMessage = "Email ou mot de passe incorrect.";
            }
            else
            {
                // Exemple : mise à jour de la date de connexion
                user.UpdateLastLogin();
                await App.Database.UpdateAsync(user);

                // ✅ Redirection vers la page principale (ou tableau de bord)
                await Shell.Current.GoToAsync("//MainPage");
            }

            IsBusy = false;
        }
    }
}
