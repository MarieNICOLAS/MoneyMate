using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyMate.Services;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MoneyMate.ViewModels.AuthViewModel
{
    public partial class SignupViewModel : BaseViewModel
    {
        // 1. PROPRIÉTÉS OBSERVABLES
        
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        // Message affiché dans la page
        [ObservableProperty]
        private string message = string.Empty;

        [ObservableProperty]
        private Color messageColor = Colors.Transparent;


        // 2. CONSTRUCTEUR (DÉPENDANCES)
        public SignupViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService)
            : base(authService, budgetService, expenseService, categoryService)
        {
        }


        // 3. COMMANDES

        [RelayCommand]
        private async Task SignupAsync()
        {
            await RunSafeAsync(async () =>
            {
                Message = string.Empty;
                MessageColor = Colors.Transparent;

                // === VALIDATION ===
                if (string.IsNullOrWhiteSpace(Name) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Password))
                {
                    Message = "Veuillez remplir tous les champs.";
                    MessageColor = Colors.Red;
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    Message = "Adresse email invalide.";
                    MessageColor = Colors.Red;
                    return;
                }

                if (!IsStrongPassword(Password))
                {
                    Message = "Mot de passe trop faible.";
                    MessageColor = Colors.Red;
                    return;
                }

                // === INSCRIPTION ===
                var result = await _authService.RegisterAsync(Email, Password, Name);

                if (!result)
                {
                    Message = "⚠️ Cet email est déjà utilisé.";
                    MessageColor = Colors.Red;
                    return;
                }

                // === SUCCÈS ===
                Message = "✅ Compte créé avec succès ! Redirection...";
                MessageColor = Colors.Green;

                // Nettoyage des champs
                Name = Email = Password = string.Empty;

                await Task.Delay(1000);
                await Shell.Current.GoToAsync("//LoginPage");
            });
        }


        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }


        // 4. VALIDATIONS
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Validation complète conforme sécurité (behavior)
        private bool IsStrongPassword(string password)
        {
            if (password.Length < 8)
                return false;

            if (!Regex.IsMatch(password, "[A-Z]")) // majuscule
                return false;

            if (!Regex.IsMatch(password, "[0-9]")) // chiffre
                return false;

            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]")) // caractère spécial
                return false;

            return true;
        }
    }
}