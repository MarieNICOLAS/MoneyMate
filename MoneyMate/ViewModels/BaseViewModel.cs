using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Diagnostics;

namespace MoneyMate.ViewModels
{
    /// <summary>
    /// ViewModel de base utilisé par tout le projet MoneyMate.
    /// Gère :
    /// - Injection des services
    /// - Gestion utilisateur
    /// - Messages (succès, erreurs, warning)
    /// - IsBusy / IsNotBusy
    /// - Cycle de vie OnAppear / OnDisappear
    /// - Méthodes de navigation & alerts
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        // 1. DÉPENDANCES (injectées automatiquement)
        protected readonly AuthService _authService;
        protected readonly BudgetService _budgetService;
        protected readonly ExpenseService _expenseService;
        protected readonly CategoryService _categoryService;

        // 2. PROPRIÉTÉS OBSERVABLES (réactives)
        
        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private User? currentUser;

        [ObservableProperty]
        private string message = string.Empty;

        [ObservableProperty]
        private Color messageColor = Colors.Transparent;

        // ---------------------------------------------------------
        // 3. PROPRIÉTÉS CALCULÉES
        // ---------------------------------------------------------
        public bool IsNotBusy => !IsBusy;
        public bool IsAuthenticated => CurrentUser != null;
        public int CurrentUserId => CurrentUser?.Id ?? 0;

        public string UserName => CurrentUser?.Name ?? "Invité";


        // 4. CONSTRUCTEUR (Injection de dépendances)
        public BaseViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService)
        {
            _authService = authService;
            _budgetService = budgetService;
            _expenseService = expenseService;
            _categoryService = categoryService;
        }


        // 5. COMMANDES
        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (IsBusy) return;

            try
            {
                bool confirm = await ShowConfirmAlert("Déconnexion", "Voulez-vous vraiment vous déconnecter ?");
                if (!confirm) return;

                IsBusy = true;

                AuthService.Logout();
                CurrentUser = null;

                await Shell.Current.GoToAsync("//MainPage");

                await ShowSuccessMessage("Déconnexion réussie");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Erreur logout : {ex.Message}");
                await ShowErrorMessage("Erreur lors de la déconnexion.");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // 6. MÉTHODES COMMUNES AUX VIEWMODELS
        
        /// Charge automatiquement l'utilisateur connecté
        protected async Task LoadCurrentUserAsync()
        {
            try
            {
                CurrentUser = await _authService.GetLoggedInUserAsync();

                if (CurrentUser != null)
                {
                    Debug.WriteLine($"👤 Utilisateur chargé : {CurrentUser.Name} (ID: {CurrentUser.Id})");
                }
                else
                {
                    Debug.WriteLine("⚠ Aucun utilisateur connecté");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Erreur lors du chargement utilisateur : {ex.Message}");
                CurrentUser = null;
            }
        }


        // ---------------------- Messages -------------------------

        protected async Task ShowSuccessMessage(string text)
        {
            Message = text;
            MessageColor = Colors.Green;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Succès", text, "OK");
            });

            await Task.Delay(3000);
            ClearMessage();
        }

        protected async Task ShowErrorMessage(string text)
        {
            Message = text;
            MessageColor = Colors.Red;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", text, "OK");
            });

            await Task.Delay(5000);
            ClearMessage();
        }

        protected void ShowWarningMessage(string text)
        {
            Message = text;
            MessageColor = Colors.Orange;
        }

        protected void ShowMessage(string text, Color color)
        {
            Message = text;
            MessageColor = color;
        }

        protected void ClearMessage()
        {
            Message = string.Empty;
            MessageColor = Colors.Transparent;
        }

        // ---------------------- Alerts -------------------------

        protected async Task ShowAlert(string title, string messageText)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(title, messageText, "OK");
            });
        }

        protected async Task<bool> ShowConfirmAlert(string title, string messageText)
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                return await Application.Current.MainPage.DisplayAlert(title, messageText, "Oui", "Non");
            });
        }


        // ------------------ Auth Guard -------------------------

        protected async Task<bool> EnsureAuthenticatedAsync()
        {
            if (!IsAuthenticated)
            {
                await ShowAlert("Authentification requise", "Veuillez vous connecter.");
                await Shell.Current.GoToAsync("//LoginPage");
                return false;
            }
            return true;
        }

        // EXECUTION SÉCURISÉE D'UNE ACTION ASYNCHRONE
        protected async Task RunSafeAsync(Func<Task> action, bool showError = true)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                await action();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Erreur : {ex.Message}");

                if (showError)
                    await ShowErrorMessage("Une erreur est survenue.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // 7. CYCLE DE VIE (appelé depuis les pages)
        public virtual async Task OnAppearAsync()
        {
            await LoadCurrentUserAsync();
        }

        public virtual Task OnDisappearAsync()
        {
            ClearMessage();
            return Task.CompletedTask;
        }
    }
}
