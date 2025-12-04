using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyMate.ViewModels
{
    public class ExpenseViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly BudgetService _budgetService;
        private readonly CategoryService _categoryService;

        // --- Champs privés ---
        private double amount;
        private Budget selectedBudget;
        private Category selectedCategory;
        private string description;
        private DateTime date = DateTime.Now;
        private string message;
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
                    LoadCategories(); 
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

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public Color MessageColor
        {
            get => messageColor;
            set => SetProperty(ref messageColor, value);
        }

        // --- Commandes ---
        public IRelayCommand AddExpenseCommand { get; }

        // --- Constructeur ---
        public ExpenseViewModel(ExpenseService expenseService, BudgetService budgetService, CategoryService categoryService)
        {
            _expenseService = expenseService;
            _budgetService = budgetService;
            _categoryService = categoryService;

            AddExpenseCommand = new AsyncRelayCommand(AddExpenseAsync);
            LoadBudgets();

        }

        // --- Méthodes ---
        private async Task LoadBudgets()
        {
            var budgets = await _budgetService.GetBudgetsAsync();
            Budgets.Clear();
            foreach (var b in budgets)
                Budgets.Add(b);
        }

        private async Task LoadCategories()
        {
            Categories.Clear();
            if (SelectedBudget != null)
            {
                var categories = await _categoryService.GetCategoriesByBudgetAsync(SelectedBudget.Id);
                foreach (var c in categories)
                    Categories.Add(c);
            }
        }

        private async Task AddExpenseAsync()
        {
            // Validation
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
                Date = Date,
                CreatedAt = DateTime.Now
            };

            try
            {
                await _expenseService.AddExpenseAsync(expense);
                ShowMessage("Dépense ajoutée avec succès !", Colors.Green);

                // Reset formulaire
                Amount = 0;
                SelectedBudget = null;
                SelectedCategory = null;
                Description = string.Empty;
                Date = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowMessage($"Erreur : {ex.Message}", Colors.Red);
            }
        }

        private void ShowMessage(string text, Color color)
        {
            Message = text;
            MessageColor = color;
        }
    }
}
