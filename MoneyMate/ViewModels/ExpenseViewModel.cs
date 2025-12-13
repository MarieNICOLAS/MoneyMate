using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;

namespace MoneyMate.ViewModels
{
    public partial class ExpenseViewModel : BaseViewModel
    {
        // --- Champs privés ---
        private double amount;
        private Budget selectedBudget;
        private Category selectedCategory;
        private string description;
        private DateTime date = DateTime.Now;

        [ObservableProperty]
        private int expenseId;
        
        [RelayCommand]
        private async Task SaveAsync() => await UpdateExpenseAsync();

        [RelayCommand]
        private async Task RemoveAsync() => await DeleteExpenseAsync();


        [ObservableProperty]
        private string message;

        [ObservableProperty]
        private Color messageColor = Colors.Transparent;

        // --- Collections ---
        public ObservableCollection<Budget> Budgets { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        // --- Propriétés bindées ---
        public double Amount
        {
            get => amount;
            set => SetProperty(ref amount, value);
        }

        public Budget SelectedBudget
        {
            get => selectedBudget;
            set
            {
                if (SetProperty(ref selectedBudget, value))
                    _ = LoadCategoriesAsync();
            }
        }

        public Category SelectedCategory
        {
            get => selectedCategory;
            set => SetProperty(ref selectedCategory, value);
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public DateTime Date
        {
            get => date;
            set => SetProperty(ref date, value);
        }

        // --- Commande ---

        public IRelayCommand AddExpenseCommand { get; }


        // --- Constructeur CORRECT ---
        public ExpenseViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService)
            : base(authService, budgetService, expenseService, categoryService)
        {
            AddExpenseCommand = new AsyncRelayCommand(AddExpenseAsync);
            _ = LoadBudgetsAsync();
        }

        // --- Chargement des budgets ---
        private async Task LoadBudgetsAsync()
        {
            await RunSafeAsync(async () =>
            {
                Budgets.Clear();
                var budgets = await _budgetService.GetBudgetsAsync();

                foreach (var b in budgets)
                    Budgets.Add(b);
            });
        }

        // --- Chargement des catégories ---
        private async Task LoadCategoriesAsync()
        {
            Categories.Clear();

            if (SelectedBudget == null)
                return;

            await RunSafeAsync(async () =>
            {
                var categories = await _categoryService.GetCategoriesByBudgetAsync(SelectedBudget.Id);
                foreach (var c in categories)
                    Categories.Add(c);
            });
        }

       
        // --- Ajout d’une dépense ---
        private async Task AddExpenseAsync()
        {
            await RunSafeAsync(async () =>
            {
                // Validations
                if (SelectedBudget == null)
                {
                    ShowMessage("Veuillez sélectionner un budget.", Colors.Red);
                    return;
                }

                if (SelectedCategory == null)
                {
                    ShowMessage("Veuillez sélectionner une catégorie.", Colors.Red);
                    return;
                }

                if (Amount <= 0)
                {
                    ShowMessage("Le montant doit être supérieur à 0.", Colors.Red);
                    return;
                }

                var expense = new Expense
                {
                    BudgetId = SelectedBudget.Id,
                    CategoryId = SelectedCategory.Id,
                    Amount = Amount,
                    Description = Description,
                    CreatedAt = DateTime.Now,
                    Date = Date
                };

                await _expenseService.AddExpenseAsync(expense);

                ShowMessage("Dépense ajoutée avec succès !", Colors.Green);

                // Reset
                Amount = 0;
                SelectedBudget = null;
                SelectedCategory = null;
                Description = string.Empty;
                Date = DateTime.Now;
            });
        }
        public async Task LoadExpenseAsync()
        {
            await RunSafeAsync(async () =>
            {
                var expense = await _expenseService.GetByIdAsync(ExpenseId);
                if (expense == null)
                {
                    ShowMessage("Dépense introuvable.", Colors.Red);
                    return;
                }

                // Pré-remplissage UI
                Amount = expense.Amount;
                Description = expense.Description;
                Date = expense.Date;

                // Charger budgets
                await LoadBudgetsAsync();
                SelectedBudget = Budgets.FirstOrDefault(b => b.Id == expense.BudgetId);

                // Charger catégories du budget
                await LoadCategoriesAsync();
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == expense.CategoryId);
            });
        }

        public async Task UpdateExpenseAsync()
        {
            await RunSafeAsync(async () =>
            {
                if (SelectedBudget == null || SelectedCategory == null)
                {
                    ShowMessage("Veuillez sélectionner un budget et une catégorie.", Colors.Red);
                    return;
                }

                var expense = await _expenseService.GetByIdAsync(ExpenseId);
                if (expense == null)
                {
                    ShowMessage("Erreur : dépense introuvable.", Colors.Red);
                    return;
                }

                expense.Amount = Amount;
                expense.Description = Description;
                expense.Date = Date;
                expense.BudgetId = SelectedBudget.Id;
                expense.CategoryId = SelectedCategory.Id;

                await _expenseService.UpdateExpenseAsync(expense);

                ShowMessage("Dépense mise à jour !", Colors.Green);

                await Shell.Current.GoToAsync("..");
            });
        }

        public async Task DeleteExpenseAsync()
        {
            await RunSafeAsync(async () =>
            {
                bool confirm = await ShowConfirmAlert("Supprimer",
                    "Voulez-vous vraiment supprimer cette dépense ?");

                if (!confirm) return;

                var expense = await _expenseService.GetByIdAsync(ExpenseId);
                if (expense == null)
                {
                    ShowMessage("Erreur : dépense introuvable.", Colors.Red);
                    return;
                }

                await _expenseService.DeleteExpenseAsync(expense);

                ShowMessage("Dépense supprimée.", Colors.Green);

                await Shell.Current.GoToAsync("..");
            });
        }

    }
}
