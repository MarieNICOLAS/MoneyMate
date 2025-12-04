using CommunityToolkit.Mvvm.Input;
using MoneyMate.Database;
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
    public class CategoryViewModel : BaseViewModel
    {
        private readonly MoneyMateContext _context;
        private readonly CategoryService _categoryService;
        private readonly BudgetService _budgetService;

        // --- Champs privés ---
        private string name;
        private Budget selectedBudget;
        private string selectedColor;
        private double percentage;

        private string message;
        private Color messageColor = Colors.Transparent;

        // --- Collections ---
        public ObservableCollection<Budget> Budgets { get; } = new();
        public ObservableCollection<string> ColorOptions { get; } = new()
        {
            "#FFAFAD", "#FFD6A5", "#FCFEB6",
            "#CAFEBF", "#9DF3FD", "#A2C3FD",
            "#BFB3FD", "#FFC7FC"
        });

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private Budget? _selectedBudget;
        public Budget? SelectedBudget
        {
            get => _selectedBudget;
            set
            {
                if (SetProperty(ref _selectedBudget, value))
                    OnPropertyChanged(nameof(AmountPreview));
            }
        }

        private string? _selectedColor;
        public string? SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        private double _percentage = 10;
        public double Percentage
        {
            get => percentage;
            set => SetProperty(ref percentage, value);
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

        // 💶 Aperçu du montant selon budget + %
        public double AmountPreview =>
            SelectedBudget == null
                ? 0
                : SelectedBudget.TotalAmount * (Percentage / 100.0);

        // COMMANDES
        public IAsyncRelayCommand AddCategoryCommand { get; }
        public IAsyncRelayCommand UpdateCategoryCommand { get; }
        public IAsyncRelayCommand DeleteCategoryCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // ⭐ Navbar commands
        public IRelayCommand GoHomeCommand { get; }
        public IRelayCommand GoStatisticsCommand { get; }
        public IRelayCommand GoAddCommand { get; }
        public IRelayCommand GoSearchCommand { get; }
        public IRelayCommand GoMenuCommand { get; }

        public CategoryViewModel()
        {
            _categoryService = categoryService;
            _budgetService = budgetService;

            SelectedColor = null;

            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
            UpdateCategoryCommand = new AsyncRelayCommand(UpdateCategoryAsync);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync);
            CancelCommand = new RelayCommand(Cancel);

            LoadBudgets();
        }

        // Charger les budgets existants
        private async void LoadBudgets()
        {
            var budgets = await _budgetService.GetBudgetsAsync();

            Budgets.Clear();
            foreach (var b in budgets)
                Budgets.Add(b);
        }

        // --- Création d’une catégorie ---
        private async Task AddCategoryAsync()
        {
            // --- VALIDATION DES CHAMPS ---
            if (string.IsNullOrWhiteSpace(Name))
        {
                ShowMessage("Veuillez saisir un nom de catégorie.", Colors.Red);
                return;
        }

            if (SelectedBudget == null)
        {
                ShowMessage("Veuillez sélectionner un budget.", Colors.Red);
                return;
        }

            if (string.IsNullOrWhiteSpace(SelectedColor))
        {
                ShowMessage("Veuillez choisir une couleur.", Colors.Red);
                return;
            }

            if (Percentage <= 0)
            {
                ShowMessage("Le pourcentage doit être supérieur à 0.", Colors.Red);
                return;
            }

            // --- LOGIQUE CATÉGORIE ---
                var category = new Category
                {
                    BudgetId = SelectedBudget.Id,
                Name = Name,
                ColorHex = SelectedColor,
                    Percentage = Percentage,
                AllocatedAmount = SelectedBudget.TotalAmount * (Percentage / 100),
                SpentAmount = 0,
                CreatedAt = DateTime.Now
                };

                category.CalculateAllocatedAmount(SelectedBudget.TotalAmount);

                await _categoryService.AddCategoryAsync(category);
                Categories.Add(category);

                ResetForm();
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- UPDATE ---
        private async Task UpdateCategoryAsync()
        {
            if (IsBusy || SelectedBudget is null)
                return;

            try
            {
                IsBusy = true;

                var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
                if (category == null) return;

                category.Name = Name;
                category.Percentage = Percentage;
                category.ColorHex = SelectedColor ?? "#CCCCCC";
                category.BudgetId = SelectedBudget.Id;

                category.CalculateAllocatedAmount(SelectedBudget.TotalAmount);

                // Reset formulaire
                Name = string.Empty;
                SelectedBudget = null;
                Percentage = 0;
                SelectedColor = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowMessage($"Erreur : {ex.Message}", Colors.Red);
            }
        }

        // --- DELETE ---
        private async Task DeleteCategoryAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
                if (category == null) return;

                await _categoryService.DeleteCategoryAsync(category);
                Categories.Remove(category);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- CANCEL ---
        private void Cancel()
        {
            ResetForm();
            Shell.Current.GoToAsync("..");
        }

        private void ResetForm()
        {
            Name = string.Empty;
            Percentage = 10;
            SelectedColor = null;
            OnPropertyChanged(nameof(AmountPreview));
        }

        // Id de la catégorie en cours d'édition
        public int EditingCategoryId { get; set; }
    }
}
