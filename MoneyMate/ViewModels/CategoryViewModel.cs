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
    public class CategoryViewModel : BaseViewModel
    {
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
            "#FF5733", "#33C1FF", "#75FF33", "#FF33A8", "#FFC300", "#8E44AD"
        };

        // --- Propriétés bindées ---
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public Budget SelectedBudget
        {
            get => selectedBudget;
            set => SetProperty(ref selectedBudget, value);
        }

        public string SelectedColor
        {
            get => selectedColor;
            set => SetProperty(ref selectedColor, value);
        }

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

        // --- Commandes ---
        public IRelayCommand AddCategoryCommand { get; }

        // --- Constructeur ---
        public CategoryViewModel(CategoryService categoryService, BudgetService budgetService)
        {
            _categoryService = categoryService;
            _budgetService = budgetService;

            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);

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

            try
            {
                await _categoryService.AddCategoryAsync(category);

                ShowMessage("Catégorie ajoutée avec succès !", Colors.Green);

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

        private void ShowMessage(string txt, Color color)
        {
            Message = txt;
            MessageColor = color;
        }
    }
}

