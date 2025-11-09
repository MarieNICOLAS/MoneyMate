using System.Windows.Input;
using MoneyMate.Services;
using Microsoft.Maui.Controls;
using MoneyMate.Models;

namespace MoneyMate.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private string email;
        private string password;
        private string errorMessage;
        private bool rememberMe;

        public string Email
        {
            get => email;
            set { if (email != value) { email = value; OnPropertyChanged(); } }
        }

        public string Password
        {
            get => password;
            set { if (password != value) { password = value; OnPropertyChanged(); } }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set { if (errorMessage != value) { errorMessage = value; OnPropertyChanged(); } }
        }

        public bool RememberMe
        {
            get => rememberMe;
            set { if (rememberMe != value) { rememberMe = value; OnPropertyChanged(); } }
        }

        // 🔘 Commandes MVVM
        public ICommand LoginCommand { get; }
        public ICommand GoToSignupCommand { get; }
        public ICommand OnBackClickedCommand { get; } 

        public LoginViewModel()
        {
            _authService = new AuthService(App.Database);

            LoginCommand = new Command(async () => await LoginAsync());
            GoToSignupCommand = new Command(async () => await Shell.Current.GoToAsync("//SignupPage"));
            OnBackClickedCommand = new Command(async () => await Shell.Current.GoToAsync("//MainPage")); 
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

            var user = await _authService.LoginAsync(Email, Password, RememberMe);

            if (user == null)
            {
                ErrorMessage = "Email ou mot de passe incorrect.";
            }
            else
            {
                user.UpdateLastLogin();
                await App.Database.UpdateAsync(user);

                // ✅ Redirection après connexion réussie
                await Shell.Current.GoToAsync("//DashboardPage");
            }

            IsBusy = false;
        }
    }
}
