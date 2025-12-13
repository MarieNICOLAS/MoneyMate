using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;

namespace MoneyMate.ViewModels
{
    public class BudgetCategoryViewModel : BaseViewModel
    {
        private readonly BudgetCategoryService _pivotService;
        private readonly CategoryService _categoryService;
        private readonly BudgetService _budgetService;

        // -----------------------------
        // PROPRIÉTÉS
        // -----------------------------

        private int _budgetId;
        public int BudgetId
        {
            get => _budgetId;
            set => SetProperty(ref _budgetId, value);
        }

        private Category _selectedCategory = null!;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    OnPropertyChanged(nameof(IsEditMode));
            }
        }

        private BudgetCategory _editingPivot = null!;
        public BudgetCategory EditingPivot
        {
            get => _editingPivot;
            set
            {
                if (SetProperty(ref _editingPivot, value))
                {
                    if (value != null)
                    {
                        Percentage = EstimatePercentage(value);
                        SelectedColor = value.ColorHex;
                    }

                    OnPropertyChanged(nameof(IsEditMode));
                }
            }
        }

        private double _percentage = 10;
        public double Percentage
        {
            get => _percentage;
            set
            {
                if (SetProperty(ref _percentage, value))
                    OnPropertyChanged(nameof(AmountPreview));
            }
        }

        private string _selectedColor = "#CCCCCC";
        public string SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        private double _budgetTotalAmount;
        public double BudgetTotalAmount
        {
            get => _budgetTotalAmount;
            set
            {
                if (SetProperty(ref _budgetTotalAmount, value))
                    OnPropertyChanged(nameof(AmountPreview));
            }
        }

        // Montant preview → totalAmount × percentage / 100
        public double AmountPreview =>
            Math.Round(BudgetTotalAmount * (Percentage / 100.0), 2);

        // Affiche si on édite un pivot existant
        public bool IsEditMode => EditingPivot != null;

        // Liste des catégories globales
        public ObservableCollection<Category> Categories { get; } = new();

        // -----------------------------
        // COMMANDES
        // -----------------------------
        public IAsyncRelayCommand SaveCategoryToBudgetCommand { get; }
        public IAsyncRelayCommand DeletePivotCommand { get; }

        // -----------------------------
        // CONSTRUCTEUR
        // -----------------------------
        public BudgetCategoryViewModel()
        {
            _pivotService = new BudgetCategoryService(App.Database);
            _categoryService = new CategoryService(App.Database);
            _budgetService = new BudgetService(App.Database);

            SaveCategoryToBudgetCommand = new AsyncRelayCommand(SaveAsync);
            DeletePivotCommand = new AsyncRelayCommand(DeleteAsync);
        }

        // -----------------------------
        // INITIALISATION
        // -----------------------------
        public async Task InitializeAsync(int budgetId)
        {
            BudgetId = budgetId;

            var budget = await _budgetService.GetBudgetByIdAsync(budgetId);
            if (budget != null)
            {
                BudgetTotalAmount = budget.TotalAmount;
            }

            var categories = await _categoryService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(c);
        }

        // -----------------------------
        // AJOUT / ÉDITION
        // -----------------------------
        private async Task SaveAsync()
        {
            if (IsBusy)
                return;

            if (SelectedCategory == null)
            {
                ShowMessage("Veuillez choisir une catégorie.", Colors.Red);
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

                if (!IsEditMode)
                {
                    // Ajout d'une nouvelle catégorie au budget
                    await _pivotService.AddCategoryToBudgetAsync(
                        BudgetId,
                        SelectedCategory.Id,
                        Percentage,
                        SelectedColor
                    );

                    ShowMessage("Catégorie ajoutée au budget !", Colors.Green);
                }
                else
                {
                    // Modification d'un pivot existant
                    EditingPivot.ColorHex = SelectedColor;
                    await _pivotService.UpdateBudgetCategoryAsync(EditingPivot, Percentage);

                    ShowMessage("Catégorie mise à jour.", Colors.Green);
                }
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

        // -----------------------------
        // SUPPRESSION
        // -----------------------------
        private async Task DeleteAsync()
        {
            if (!IsEditMode)
            {
                ShowMessage("Aucune catégorie à supprimer.", Colors.Red);
                return;
            }

            await _pivotService.DeleteBudgetCategoryAsync(EditingPivot);
            ShowMessage("Catégorie supprimée du budget.", Colors.Green);
        }

        // -----------------------------
        // UTILITAIRE
        // -----------------------------
        private double EstimatePercentage(BudgetCategory pivot)
        {
            if (BudgetTotalAmount <= 0)
                return 0;

            return Math.Round((pivot.AllocatedAmount / BudgetTotalAmount) * 100.0, 2);
        }

        private void ShowMessage(string text, Color color)
        {
            Message = text;
            MessageColor = color;
        }

        private string _message = string.Empty;
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
    }
}
