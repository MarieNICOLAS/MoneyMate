using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyMate.Services;

namespace MoneyMate.ViewModels.AuthViewModel
{
    public partial class LoginViewModel : BaseViewModel
    {
        // PROPRIÉTÉS OBSERVABLES (automatiquement générées)
        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        // CONSTRUCTEUR
        public LoginViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService)
            : base(authService, budgetService, expenseService, categoryService)
        {
        }

        // COMMANDES
        [RelayCommand]
        private async Task LoginAsync()
        {
            await RunSafeAsync(async () =>
            {
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Veuillez remplir tous les champs.";
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    ErrorMessage = "Adresse email invalide.";
                    return;
                }

                if (Password.Length < 6)
                {
                    ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.";
                    return;
                }

                var user = await _authService.LoginAsync(Email, Password, RememberMe);

                if (user == null)
                {
                    ErrorMessage = "Email ou mot de passe incorrect.";
                    return;
                }

                await ShowSuccessMessage("Connexion réussie !");
                await Shell.Current.GoToAsync("//DashboardPage");
            });
        }

        [RelayCommand]
        private async Task GoToSignupAsync() =>
            await Shell.Current.GoToAsync("//SignupPage");

        [RelayCommand]
        private async Task GoBackAsync() =>
            await Shell.Current.GoToAsync("//MainPage");

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch { return false; }
        }
    }
}
