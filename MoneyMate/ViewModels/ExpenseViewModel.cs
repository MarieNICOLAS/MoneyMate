using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;

namespace MoneyMate.ViewModels
{
    public class ExpenseViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly BudgetService _budgetService;
        private readonly BudgetCategoryService _pivotService;

        // -----------------------------
        // PROPRIÉTÉS
        // -----------------------------

        private Budget? _selectedBudget;
        public Budget? SelectedBudget
        {
            get => _selectedBudget;
            set
            {
                if (SetProperty(ref _selectedBudget, value))
                {
                    _ = LoadBudgetCategories();
                }
            }
        }

        private (BudgetCategory pivot, Category category)? _selectedPivot;
        public (BudgetCategory pivot, Category category)? SelectedPivot
        {
            get => _selectedPivot;
            set => SetProperty(ref _selectedPivot, value);
        }

        private double _amount;
        public double Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        // Pour affichage liste des budgets
        public ObservableCollection<Budget> Budgets { get; } = new();

        // Pour affichage liste des catégories du budget
        public ObservableCollection<(BudgetCategory pivot, Category category)> BudgetCategories { get; } = new();

        // -----------------------------
        // COMMANDES
        // -----------------------------
        public IAsyncRelayCommand AddExpenseCommand { get; }

        // -----------------------------
        // CONSTRUCTEUR
        // -----------------------------
        public ExpenseViewModel()
        {
            _expenseService = new ExpenseService(App.Database);
            _budgetService = new BudgetService(App.Database);
            _pivotService = new BudgetCategoryService(App.Database);

            AddExpenseCommand = new AsyncRelayCommand(AddExpenseAsync);

            _ = InitializeAsync();
        }

        // -----------------------------
        // INITIALISATION
        // -----------------------------
        private async Task InitializeAsync()
        {
            var budgets = await _budgetService.GetBudgetsAsync();

            Budgets.Clear();
            foreach (var b in budgets.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month))
                Budgets.Add(b);

            SelectedBudget = Budgets.FirstOrDefault();
        }

        // -----------------------------
        // CHARGEMENT DES PIVOTS (BudgetCategory)
        // -----------------------------
        private async Task LoadBudgetCategories()
        {
            if (SelectedBudget == null)
                return;

            var pivots = await _pivotService.GetCategoriesForBudgetAsync(SelectedBudget.Id);

            BudgetCategories.Clear();

            foreach (var p in pivots)
                BudgetCategories.Add(p);

            SelectedPivot = null;
        }

        // -----------------------------
        // AJOUT D'UNE DÉPENSE
        // -----------------------------
        private async Task AddExpenseAsync()
        {
            if (IsBusy)
                return;

            if (SelectedBudget == null)
            {
                ShowMessage("Veuillez sélectionner un budget.", Colors.Red);
                return;
            }

            if (SelectedPivot == null)
            {
                ShowMessage("Veuillez sélectionner une catégorie.", Colors.Red);
                return;
            }

            if (Amount <= 0)
            {
                ShowMessage("Veuillez entrer un montant valide.", Colors.Red);
                return;
            }

            try
            {
                IsBusy = true;

                var expense = new Expense
                {
                    BudgetId = SelectedBudget.Id,
                    BudgetCategoryId = SelectedPivot.Value.pivot.Id,
                    CategoryId = SelectedPivot.Value.category.Id,
                    Amount = Amount,
                    Description = Description?.Trim() ?? "",
                    Date = Date,
                    CreatedAt = DateTime.Now
                };

                await _expenseService.AddExpenseAsync(expense);

                ShowMessage("Dépense ajoutée avec succès.", Colors.Green);
                ResetForm();
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
        // RESET FORMULAIRE
        // -----------------------------
        private void ResetForm()
        {
            Amount = 0;
            Description = "";
            Date = DateTime.Now;
            SelectedPivot = null;
        }

        // -----------------------------
        // MESSAGE UI
        // -----------------------------
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

        private void ShowMessage(string text, Color color)
        {
            Message = text;
            MessageColor = color;
        }
    }
}
