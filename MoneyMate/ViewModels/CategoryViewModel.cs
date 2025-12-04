using CommunityToolkit.Mvvm.Input;
using MoneyMate.Database;
using MoneyMate.Models;
using MoneyMate.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyMate.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        private readonly CategoryService _categoryService;
        private readonly BudgetService _budgetService;

        // -------------------------
        //        PROPRIÉTÉS
        // -------------------------

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
            get => _percentage;
            set
            {
                if (SetProperty(ref _percentage, value))
                {
                    OnPropertyChanged(nameof(AmountPreview));
                }
            }
        }

        // 💶 Aperçu du montant basé sur le budget sélectionné
        public double AmountPreview =>
            SelectedBudget == null
                ? 0
                : SelectedBudget.TotalAmount * (Percentage / 100.0);

        // Messages UX
        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private Color _messageColor = Colors.Transparent;
        public Color MessageColor
        {
            get => _messageColor;
            set => SetProperty(ref _messageColor, value);
        }

        // -------------------------
        //        COLLECTIONS
        // -------------------------

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Budget> Budgets { get; } = new();

        public ObservableCollection<string> ColorOptions { get; } = new()
        {
            "#FFAFAD", "#FFD6A5", "#FCFEB6",
            "#CAFEBF", "#9DF3FD", "#A2C3FD",
            "#BFB3FD", "#FFC7FC"
        };

        // Id de la catégorie en cours d’édition
        public int EditingCategoryId { get; set; }

        // -------------------------
        //        COMMANDES
        // -------------------------
        public IAsyncRelayCommand AddCategoryCommand { get; }
        public IAsyncRelayCommand UpdateCategoryCommand { get; }
        public IAsyncRelayCommand DeleteCategoryCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // -------------------------
        //        CONSTRUCTEUR
        // -------------------------

        public CategoryViewModel()
        {
            _categoryService = new CategoryService(App.Database);
            _budgetService = new BudgetService(App.Database);

            SelectedColor = null;

            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
            UpdateCategoryCommand = new AsyncRelayCommand(UpdateCategoryAsync);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync);
            CancelCommand = new RelayCommand(Cancel);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadBudgetsAsync();
            await LoadCategoriesAsync();
        }

        // -------------------------
        //      CHARGEMENT DONNÉES
        // -------------------------

        private async Task LoadBudgetsAsync()
        {
            var list = await _budgetService.GetBudgetsAsync();

            Budgets.Clear();
            foreach (var b in list.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
                Budgets.Add(b);

            SelectedBudget ??= Budgets.FirstOrDefault();

            OnPropertyChanged(nameof(AmountPreview));
        }

        private async Task LoadCategoriesAsync()
        {
            var list = await _categoryService.GetCategoriesAsync();

            Categories.Clear();
            foreach (var c in list)
                Categories.Add(c);
        }

        // -------------------------
        //        AJOUT
        // -------------------------

        private async Task AddCategoryAsync()
        {
            if (IsBusy) return;

            // --- VALIDATIONS ---
            if (string.IsNullOrWhiteSpace(Name))
            {
                ShowMessage("Veuillez saisir un nom.", Colors.Red);
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

            try
            {
                IsBusy = true;

                var category = new Category
                {
                    BudgetId = SelectedBudget.Id,
                    Name = Name.Trim(),
                    ColorHex = SelectedColor ?? "#CCCCCC",
                    Percentage = Percentage,
                    CreatedAt = DateTime.Now,
                    SpentAmount = 0
                };

                category.CalculateAllocatedAmount(SelectedBudget.TotalAmount);

                await _categoryService.AddCategoryAsync(category);
                Categories.Add(category);

                ResetForm();
                ShowMessage("Catégorie créée avec succès.", Colors.Green);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, Colors.Red);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        //        MODIFICATION
        // -------------------------

        private async Task UpdateCategoryAsync()
        {
            if (IsBusy || SelectedBudget == null) return;

            try
            {
                IsBusy = true;

                var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
                if (category == null)
                {
                    ShowMessage("Catégorie introuvable.", Colors.Red);
                    return;
                }

                category.Name = Name.Trim();
                category.ColorHex = SelectedColor ?? "#CCCCCC";
                category.Percentage = Percentage;
                category.BudgetId = SelectedBudget.Id;

                category.CalculateAllocatedAmount(SelectedBudget.TotalAmount);

                await _categoryService.UpdateCategoryAsync(category);

                ShowMessage("Catégorie mise à jour.", Colors.Green);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, Colors.Red);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        //        SUPPRESSION
        // -------------------------

        private async Task DeleteCategoryAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
                if (category == null)
                {
                    ShowMessage("Catégorie introuvable.", Colors.Red);
                    return;
                }

                await _categoryService.DeleteCategoryAsync(category);
                Categories.Remove(category);

                ShowMessage("Catégorie supprimée.", Colors.Green);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        //        ANNULATION
        // -------------------------

        private void Cancel()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            Name = string.Empty;
            Percentage = 10;
            SelectedColor = null;
            OnPropertyChanged(nameof(AmountPreview));
        }

        // -------------------------
        //        UTILITAIRES
        // -------------------------

        private void ShowMessage(string text, Color color)
        {
            Message = text;
            MessageColor = color;
        }
    }
}