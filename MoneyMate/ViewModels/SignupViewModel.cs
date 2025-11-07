using System.Windows.Input;
using Microsoft.Maui.Controls;
using MoneyMate.Models;
using MoneyMate.Services;

namespace MoneyMate.ViewModels
{
    public class SignupViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private string name;
        private string email;
        private string password;
        private string message;
        private Color messageColor = Colors.Transparent;

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public string Message
        {
            get => message;
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color MessageColor
        {
            get => messageColor;
            set
            {
                if (messageColor != value)
                {
                    messageColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SignupCommand { get; }

        public SignupViewModel()
        {
            _authService = new AuthService(App.Database);
            SignupCommand = new Command(async () => await SignupAsync());
        }

        private async Task SignupAsync()
        {
            IsBusy = true;
            Message = string.Empty;
            MessageColor = Colors.Transparent;

            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                Message = "Veuillez remplir tous les champs.";
                MessageColor = Colors.Red;
                IsBusy = false;
                return;
            }

            var result = await _authService.RegisterAsync(Email, Password, Name);

            if (!result)
            {
                Message = "⚠️ Cet email est déjà utilisé.";
                MessageColor = Colors.Red;
            }
            else
            {
                Message = "✅ Compte créé avec succès ! Redirection vers la connexion...";
                MessageColor = Colors.Green;

                // 🔄 Réinitialise les champs du formulaire
                Name = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(Password));

                // Petite pause UX (optionnelle)
                await Task.Delay(1200);

                // ✅ Redirection vers la page Login
                await Shell.Current.GoToAsync("//LoginPage");
            }

            IsBusy = false;
        }
    }
}
