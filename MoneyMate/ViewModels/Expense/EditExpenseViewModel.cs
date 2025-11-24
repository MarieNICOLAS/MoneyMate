using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;

namespace MoneyMate.ViewModels.Expense
{
    // Indique que cette classe reçoit l'ID de la dépense comme paramètre de requête
    [QueryProperty(nameof(ExpenseId), "expenseId")]
    public partial class EditExpenseViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly CategoryService _categoryService;
        private readonly BudgetService _budgetService;

        // --- Propriétés liées à la dépense ---
        [ObservableProperty]
        private int _expenseId; // Reçoit l'ID via QueryProperty

        [ObservableProperty]
        private Expense _expense;

        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        [ObservableProperty]
        private Category? _selectedCategory;

        public EditExpenseViewModel(ExpenseService expenseService, CategoryService categoryService, BudgetService budgetService)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
            _budgetService = budgetService;
        }

        // --- Événement déclenché à la réception de l'ExpenseId ---
        partial void OnExpenseIdChanged(int value)
        {
            LoadExpenseAsync(value);
        }

        private async void LoadExpenseAsync(int id)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var expense = await _expenseService.GetExpenseByIdAsync(id);
                if (expense != null)
                {
                    CurrentExpense = expense;

                    // Chargement des catégories pour le budget associé
                    var categories = await _categoryService.GetCategoriesForBudgetAsync(expense.BudgetId);
                    Categories.Clear();
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }

                    // Sélectionner la catégorie actuelle dans le Picker
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == expense.CategoryId);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Erreur de chargement : {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UpdateExpense()
        {
            if (IsBusy || CurrentExpense.Id == 0) return;

            // Validation simple
            if (CurrentExpense.Amount <= 0 || SelectedCategory == null)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez remplir tous les champs requis.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                // Mettre à jour les champs avant la sauvegarde
                CurrentExpense.CategoryId = SelectedCategory.Id;

                await _expenseService.UpdateExpenseAsync(CurrentExpense);

                await Shell.Current.DisplayAlert("Succès", "Dépense modifiée.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Erreur lors de la modification : {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteExpense()
        {
            if (IsBusy || CurrentExpense.Id == 0) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirmer", "Voulez-vous vraiment supprimer cette dépense ?", "Oui", "Non");

            if (confirm)
            {
                IsBusy = true;
                try
                {
                    await _expenseService.DeleteExpenseAsync(CurrentExpense);

                    await Shell.Current.DisplayAlert("Succès", "Dépense supprimée.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Erreur", $"Erreur lors de la suppression : {ex.Message}", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}