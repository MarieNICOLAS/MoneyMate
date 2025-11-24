using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System;
using System.Collections.ObjectModel;

namespace MoneyMate.ViewModels.Expense
{
    public partial class AddExpenseViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly CategoryService _categoryService;
        private readonly BudgetService _budgetService;
        private readonly AuthService _authService;

        // --- Propriétés du Formulaire ---
        [ObservableProperty]
        private double _amount;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTime _date = DateTime.Now;

        [ObservableProperty]
        private Category? _selectedCategory;

        // --- Collections ---
        public ObservableCollection<Category> Categories { get; } = new();

        // --- Données Actives ---
        private Budget? _currentBudget;
        private int? _currentUserId;
        public AddExpenseViewModel(ExpenseService expenseService, CategoryService categoryService, BudgetService budgetService, AuthService authService)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
            _budgetService = budgetService;
            _authService = authService;
            LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                // 1: Récupérer l'ID de l'utilisateur réel
                _currentUserId = await _authService.GetCurrentUserIdAsync();

                if (_currentUserId == null)
                {
                    // Si pas d'ID, la session a expiré ou l'utilisateur n'est pas connecté.
                    await Shell.Current.DisplayAlert("Session requise", "Veuillez vous connecter pour ajouter une dépense.", "OK");
                    await Shell.Current.GoToAsync("//LoginPage"); 
                    return;
                }

                // 2. Récupérer le budget actif du mois courant
                var now = DateTime.Now;
                
                //Utiliser _currentUserId.Value
                _currentBudget = await _budgetService.GetCurrentMonthBudgetAsync(_currentUserId.Value, now.Month, now.Year);

                if (_currentBudget == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Aucun budget actif trouvé pour le mois en cours. Veuillez créer un budget avant d'ajouter des dépenses.", "OK");
                    return;
                }

                // 3. Récupérer les catégories associées à ce budget
                Categories.Clear();
                var categories = await _categoryService.GetCategoriesForBudgetAsync(_currentBudget.Id);
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur de chargement", $"Impossible de charger les données : {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddExpense()
        {

            if (SelectedCategory == null)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez sélectionner une catégorie.", "OK");
                return;
            }

            // Vérifier que les informations nécessaires sont là
            if (_currentBudget == null || _currentUserId == null)
            {
                await Shell.Current.DisplayAlert("Erreur", "Session expirée ou budget non trouvé. Veuillez recharger la page.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var newExpense = new MoneyMate.Models.Expense(_currentBudget.Id, SelectedCategory.Id, Amount, Description)
                {
                    Date = Date
                };

                // Sauvegarde de la dépense
                await _expenseService.AddExpenseAsync(newExpense);

                // Succès et retour
                await Shell.Current.DisplayAlert("Succès", "Dépense ajoutée avec succès !", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Une erreur est survenue lors de l'ajout : {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
